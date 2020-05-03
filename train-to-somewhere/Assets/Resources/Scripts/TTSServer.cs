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


    void HandleMovementInput(ushort clientID, TTS.InputMovementMessage input)
    {
        ushort playerTTSID = clientPlayerMap[clientID];
        GameObject player = idMap.getTransform(playerTTSID).gameObject;
        player.GetComponent<TTSNetworkedPlayer>().SetMovementInput(input.MoveDirection, input.Dashing);
    }

    void HandleInteractInput(ushort clientID, TTS.InputInteractMessage input)
    {
        ushort playerTTSID = clientPlayerMap[clientID];
        GameObject player = idMap.getTransform(playerTTSID).gameObject;
        player.GetComponent<TTSNetworkedPlayer>().fire1ButtonDown = input.fire1;
        player.GetComponent<TTSNetworkedPlayer>().fire2ButtonDown = input.fire2;
    }

    public void ClientMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        ushort messageTag = e.GetMessage().Tag;
        switch((TTS.MessageType)messageTag)
        {
            case (TTS.MessageType.INPUT_MOVEMENT):
                using (Message m = e.GetMessage() as Message)
                    using (DarkRiftReader r = m.GetReader())
                        HandleMovementInput(e.Client.ID, r.ReadSerializable<TTS.InputMovementMessage>());
                break;

            case TTS.MessageType.INPUT_INTERACT:
                using (Message m = e.GetMessage() as Message)
                    using (DarkRiftReader r = m.GetReader())
                         HandleInteractInput(e.Client.ID, r.ReadSerializable<TTS.InputInteractMessage>());
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
