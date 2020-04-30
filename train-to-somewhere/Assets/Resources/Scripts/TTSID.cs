using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using DarkRift.Server;
using DarkRift.Server.Unity;

public class TTSID : MonoBehaviour
{
    [HideInInspector]
    public ushort id = 0;

    [Tooltip("Minimum distance delta before server will sync to clients.")]
    public float syncDistanceTrig = 0.05f;
    [Tooltip("Minimum rotation angle delta before server will sync to clients.")]
    public float syncRotationTrig = 0.05f;
    XmlUnityServer darkRiftServer;
    Vector3 lastSyncedPosition;
    Quaternion lastSyncedRotation;

    public void Init()
    {
        if (id == 0)
            id = GameObject.FindGameObjectWithTag("Network").GetComponent<TTSIDCounter>().GenerateID();
    }

    private void Start()
    {
        Init();
        darkRiftServer = GameObject.FindGameObjectWithTag("Network").GetComponent<XmlUnityServer>();
        lastSyncedPosition = transform.localPosition;
        lastSyncedRotation = transform.localRotation;
    }

    public bool ShouldSync()
    {
        return (Vector3.Distance(transform.localPosition, lastSyncedPosition) >= syncDistanceTrig) ||
               (Quaternion.Angle(lastSyncedRotation, transform.localRotation) >= syncRotationTrig);
    }

    /*
     * Syncs position & rotation to clients if ShouldSync is true
     */
    public void Sync()
    {
        if (ShouldSync())
        {
            using (DarkRiftWriter objectSyncWriter = DarkRiftWriter.Create())
            {
                objectSyncWriter.Write(new TTSGameObjectSyncMessage(id, transform));
                using (Message objectSyncMessage = Message.Create(TTSMessage.GAME_OBJECT_SYNC, objectSyncWriter))
                {
                    foreach (IClient c in darkRiftServer.Server.ClientManager.GetAllClients())
                        c.SendMessage(objectSyncMessage, SendMode.Unreliable);
                }
            }
            lastSyncedPosition = transform.localPosition;
            lastSyncedRotation = transform.localRotation;
        }
    }

    private void Update()
    {
        if (darkRiftServer != null)
            Sync();
    }
}
