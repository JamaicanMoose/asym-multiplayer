using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift.Client;
using DarkRift.Client.Unity;
using DarkRift;

public class TTSClient : TTSGeneric
{
    [HideInInspector]
    public ushort localPlayerId;

    Dictionary<ushort, Transform> idMap;

    [SerializeField]
    [Tooltip("The local player prefab.")]
    public GameObject controllablePlayerPrefab;

    UnityClient client;

    // Start is called before the first frame update
    void Start()
    {
        idMap = GameObject.FindGameObjectWithTag("Network").GetComponent<TTSIDMap>().idMap;
        client = GetComponent<UnityClient>();
        client.Client.MessageReceived += ClientMessageReceived;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ClientMessageReceived(object sender, MessageReceivedEventArgs e)
    {

        switch(e.GetMessage().Tag)
        {
            case TTSMessage.PLAYER_SYNC:
                SyncPlayer(e);
                break;
            default:
                break;
        }
    }

    void SyncPlayer(MessageReceivedEventArgs e)
    {
          
        TTSGameObjectSyncMessage syncData;
        using (Message m = e.GetMessage() as Message)
        {
            using (DarkRiftReader r = m.GetReader())
            {
                syncData = r.ReadSerializable<TTSGameObjectSyncMessage>();
            }
        }
        ushort playerID = syncData.ttsid;
        Vector3 localPosition = syncData.position;
        Quaternion localRotation = syncData.rotation;

        if(idMap.ContainsKey(playerID))
        {
            Transform toSync = idMap[playerID];
            toSync.localPosition = localPosition;
            toSync.localRotation = localRotation;  
        }

    }

    public override Transform GetLocalPlayer()
    {
        return idMap[localPlayerId];
    }
}
