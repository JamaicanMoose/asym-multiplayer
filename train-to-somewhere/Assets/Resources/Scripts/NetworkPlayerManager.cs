using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;

public class NetworkPlayerManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The DarkRift client to communicate on.")]
    UnityClient client;

    ushort MOVE_TAG = 1;

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
                    Vector3 newPosition = new Vector3(reader.ReadSingle(), 1.07f, reader.ReadSingle());

                    if (networkPlayers.ContainsKey(id))
                        networkPlayers[id].SetMovePosition(newPosition);
                }
            }
        }
    }


}
