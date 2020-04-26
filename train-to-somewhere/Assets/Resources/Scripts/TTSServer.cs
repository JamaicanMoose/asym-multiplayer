using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DarkRift.Server.Unity;
using DarkRift.Server;
using DarkRift;

public class TTSServer : MonoBehaviour
{

    //Client ID -> TTSID
    public Dictionary<ushort, ushort> clientPlayerMap;
    private TTSIDMap idMap;

  

    private void Awake()
    {
        clientPlayerMap = new Dictionary<ushort, ushort>();
        idMap = GameObject.Find("Network").GetComponent<TTSIDMap>();
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
}
