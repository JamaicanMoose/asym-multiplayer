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

    bool dashing = false;
    bool lastDashing = false;
    Vector3 lastMoveVector = Vector3.zero;
    Transform mainCameraTransform;
    


    private void Awake()
    {
        clientPlayerMap = new Dictionary<ushort, ushort>();
        idMap = GameObject.Find("Network").GetComponent<TTSIDMap>();

        darkRiftServer = GetComponent<XmlUnityServer>();

        mainCameraTransform = GameObject.FindWithTag("MainCamera").transform;
    }

    private void Update()
    {
        //Check all players for motion to send to clients
            foreach(ushort playerID in clientPlayerMap.Values)
            {
                if(Vector3.Distance(idMap.idMap[playerID].localPosition, idMap.idMap[playerID].GetComponent<TTSNetworkedPlayer>().lastSyncPostion) > playerMoveDistance)
                {
                    using (DarkRiftWriter playerPositionWriter = DarkRiftWriter.Create())
                    {

                        playerPositionWriter.Write(new TTSGameObjectSyncMessage(playerID, idMap.idMap[playerID]));
                        
                        using (Message playerPositionMessage = Message.Create(TTSMessage.GAME_OBJECT_SYNC, playerPositionWriter))
                        {
                            foreach (IClient c in darkRiftServer.Server.ClientManager.GetAllClients())
                                c.SendMessage(playerPositionMessage, SendMode.Unreliable);
                        }
                    }
               
                    idMap.idMap[playerID].GetComponent<TTSNetworkedPlayer>().lastSyncPostion = idMap.idMap[playerID].localPosition;

                }
            }

        //check for server player input and move server player
        Vector3 moveDirection = mainCameraTransform.right * Input.GetAxis("Horizontal") + mainCameraTransform.forward * Input.GetAxis("Vertical");

        moveDirection.y = 0f;
        moveDirection.Normalize();

        dashing = Input.GetKey(KeyCode.Space);

        if (moveDirection != lastMoveVector || dashing != lastDashing)
        {
            MovePlayer(65000, moveDirection, dashing);
        }

       lastMoveVector = moveDirection;
       lastDashing = dashing;
 


    }


    public void ClientMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        ushort messageTag = e.GetMessage().Tag;
        switch(messageTag)
        {
            case (TTSMessage.MOVEMENT_INPUT):
                TTSInputMessage newInput;
                using (Message m = e.GetMessage() as Message)
                {
                    using (DarkRiftReader r = m.GetReader())
                    {
                        newInput = r.ReadSerializable<TTSInputMessage>();
                    }
                }
                MovePlayer(e.Client.ID, newInput.MoveDirection, newInput.Dashing);
                break;

            default:
                Debug.LogWarning("Server received unhandled tag: " + messageTag);
                break;

      
        }
    }

    private void MovePlayer(ushort clientID, Vector3 moveDirection, bool dashing)
    {
        ushort playerTTSID = clientPlayerMap[clientID];
        GameObject player = idMap.idMap[playerTTSID].gameObject;
        player.GetComponent<TTSNetworkedPlayer>().SetVelocity(moveDirection, dashing);
    }

    public override Transform GetLocalPlayer()
    {
        return idMap.idMap[clientPlayerMap[(ushort)65000]];
    }
}
