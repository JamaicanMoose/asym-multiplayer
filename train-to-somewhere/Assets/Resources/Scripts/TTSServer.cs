using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DarkRift.Server.Unity;
using DarkRift.Server;
using DarkRift;

/*
 * TTSServer Unity Script
 * - Add script to same GameObject as DR2 XMLUnityServer & TTSIDMap
 */

public class TTSServer : TTSGeneric
{
    //Client ID -> TTSID
    public Dictionary<ushort, ushort> clientPlayerMap;
    private TTSIDMap idMap;

    XmlUnityServer darkRiftServer;


    private void Awake()
    {
        clientPlayerMap = new Dictionary<ushort, ushort>();
        idMap = GetComponent<TTSIDMap>();
        darkRiftServer = GetComponent<XmlUnityServer>();
    }

    private void Update()
    {
        
    }

    void HandleMovementInput(ushort clientID, TTSInputMessage input)
    {
        ushort playerTTSID = clientPlayerMap[clientID];
        GameObject player = idMap.idMap[playerTTSID].gameObject;
        player.GetComponent<TTSNetworkedPlayer>().SetVelocity(input.MoveDirection, input.Dashing);
    }

    public void ClientMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        ushort messageTag = e.GetMessage().Tag;
        switch(messageTag)
        {
            case (TTSMessage.MOVEMENT_INPUT):
                TTSInputMessage newInput;
                using (Message m = e.GetMessage() as Message)
                    using (DarkRiftReader r = m.GetReader())
                        HandleMovementInput(e.Client.ID, r.ReadSerializable<TTSInputMessage>());
                break;

            default:
                Debug.LogWarning("Server received unhandled tag: " + messageTag);
                break;
        }
    }

    public override Transform GetLocalPlayer()
    {
        return idMap.idMap[clientPlayerMap[(ushort)65000]];
    }
}
