using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DarkRift.Server.Unity;
using DarkRift.Server;
using DarkRift;

public class TTSServer : TTSGeneric
{

    //Client ID -> TTSID
    public Dictionary<ushort, ushort> clientPlayerMap;
    private TTSIDMap idMap;


    public float playerMoveDistance = 0.05f;

    XmlUnityServer darkRiftServer;

    private void Awake()
    {
        clientPlayerMap = new Dictionary<ushort, ushort>();
        idMap = GameObject.Find("Network").GetComponent<TTSIDMap>();

        darkRiftServer = GetComponent<XmlUnityServer>();
    }

    private void Update()
    {
            foreach(ushort playerID in clientPlayerMap.Values)
            {
                if(Vector3.Distance(idMap.idMap[playerID].localPosition, idMap.idMap[playerID].GetComponent<TTSNetworkedPlayer>().lastSyncPostion) > playerMoveDistance)
                {
                    using (DarkRiftWriter playerPositionWriter = DarkRiftWriter.Create())
                    {

                        playerPositionWriter.Write(new TTSGameObjectSyncMessage(playerID, idMap.idMap[playerID]));
                        
                        using (Message playerPositionMessage = Message.Create(TTSMessage.PLAYER_SYNC, playerPositionWriter))
                        {
                            foreach (IClient c in darkRiftServer.Server.ClientManager.GetAllClients())
                                c.SendMessage(playerPositionMessage, SendMode.Unreliable);
                        }
                    }
               
                    idMap.idMap[playerID].GetComponent<TTSNetworkedPlayer>().lastSyncPostion = idMap.idMap[playerID].localPosition;

                }
            }
        
    }


    public void ClientMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        ushort messageTag = e.GetMessage().Tag;
        switch(messageTag)
        {
            case (TTSMessage.MOVEMENT_INPUT):
                MovePlayer(e);
                break;

            default:
                Debug.LogWarning("Server received unhandled tag: " + messageTag);
                break;

      
        }
    }

    private void MovePlayer(MessageReceivedEventArgs e)
    {
        ushort clientID = e.Client.ID;
        ushort playerTTSID = clientPlayerMap[clientID];

        TTSInputMessage newInput;
        using (Message m = e.GetMessage() as Message)
        {
            using (DarkRiftReader r = m.GetReader())
            {
                 newInput = r.ReadSerializable<TTSInputMessage>();
            }
        }
        Vector3 moveDirection = newInput.MoveDirection;
        bool dashing = newInput.Dashing;

        GameObject player = idMap.idMap[playerTTSID].gameObject;


        player.GetComponent<TTSNetworkedPlayer>().SetVelocity(moveDirection, dashing);

    }

    public override Transform GetLocalPlayer()
    {
        return idMap.idMap[clientPlayerMap[(ushort)65000]];
    }
}
