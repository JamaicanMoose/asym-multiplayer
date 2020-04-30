using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift.Server.Unity;

public class TTSID : MonoBehaviour
{
    [HideInInspector]
    public ushort id = 0;

    [Tooltip("Minimum distance delta before server will sync to clients.")]
    public float syncDistanceTrig = 0.05f;
    [Tooltip("Minimum rotation angle delta before server will sync to clients.")]
    public float syncRotationTrig = 0.05f;
    bool isServer = false;
    List<TTSGameObjectSyncMessage> objectSyncBuffer;
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
        isServer = GameObject.FindGameObjectWithTag("Network").GetComponent<XmlUnityServer>() != null;
        objectSyncBuffer = GameObject.FindGameObjectWithTag("Network").GetComponent<TTS.ObjectSync>().buffer;
        lastSyncedPosition = transform.localPosition;
        lastSyncedRotation = transform.localRotation;
    }

    public bool ShouldSync()
    {
        return (Vector3.Distance(transform.localPosition, lastSyncedPosition) >= syncDistanceTrig) ||
               (Quaternion.Angle(lastSyncedRotation, transform.localRotation) >= syncRotationTrig);
    }

    private void Update()
    {
        if (isServer && ShouldSync())
        {
            objectSyncBuffer.Add(new TTSGameObjectSyncMessage(id, transform));
            lastSyncedPosition = transform.localPosition;
            lastSyncedRotation = transform.localRotation;
        }
    }
}
