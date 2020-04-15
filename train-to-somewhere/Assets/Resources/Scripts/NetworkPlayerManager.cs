using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using UnityEngine.Assertions;

public class NetworkPlayerManager : MonoBehaviour
{
    UnityClient client;

    ushort MOVE_TAG = 1;

    public static float DEFAULT_PLAYER_Y = 3.4f;

    Dictionary<ushort, PlayerObject> networkPlayers = new Dictionary<ushort, PlayerObject>();

    public void Add(ushort id, PlayerObject player)
    {
        networkPlayers.Add(id, player);
    }

    public void DestroyPlayer(ushort id)
    {
        PlayerObject obj = networkPlayers[id];
        Destroy(obj.gameObject);
        networkPlayers.Remove(id);
    }

    private void Awake()
    {
        client = gameObject.GetComponent<UnityClient>();
        Assert.IsNotNull(client);

        client.MessageReceived += MessageReceived;
    }

    void MessageReceived(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage() as Message)
        {
            if (message.Tag == MOVE_TAG)
            {         
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort id = reader.ReadUInt16();
                    Vector3 newPosition = new Vector3(reader.ReadSingle(), DEFAULT_PLAYER_Y, reader.ReadSingle());
                    Vector3 newRotation = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    if (networkPlayers.ContainsKey(id))
                    {
                        networkPlayers[id].SetMovePosition(newPosition);
                        networkPlayers[id].SetRotation(newRotation);
                    }
                        
                }
            }
        }
    }


}
