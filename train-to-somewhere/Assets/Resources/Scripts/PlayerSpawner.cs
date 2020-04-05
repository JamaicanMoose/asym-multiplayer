using System.Collections;
using System.Collections.Generic;
using DarkRift.Client.Unity;
using DarkRift.Client;
using DarkRift;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{

    ushort SPAWN_TAG = 0;
    ushort DESPAWN_TAG = 2;

    [SerializeField]
    [Tooltip("The DarkRift client to communicate on.")]
    UnityClient client;

    [SerializeField]
    [Tooltip("The controllable player prefab.")]
    GameObject controllablePrefab;

    [SerializeField]
    [Tooltip("The network controllable player prefab.")]
    GameObject networkPrefab;

    [SerializeField]
    [Tooltip("The network player manager.")]
    NetworkPlayerManager networkPlayerManager;

    void Awake()
    {
        if (client == null)
        {
            Debug.LogError("Client unassigned in PlayerSpawner.");
            Application.Quit();
        }

        if (controllablePrefab == null)
        {
            Debug.LogError("Controllable Prefab unassigned in PlayerSpawner.");
            Application.Quit();
        }

        if (networkPrefab == null)
        {
            Debug.LogError("Network Prefab unassigned in PlayerSpawner.");
            Application.Quit();
        }

        //Subscribe our MessageReceived function to the client.MessageReceived event
        client.MessageReceived += MessageReceived;
    }

    void MessageReceived(object sender, MessageReceivedEventArgs e)
    {
        if(e.GetMessage().Tag == SPAWN_TAG)
        {
            SpawnPlayer(sender, e);
        }
        else if(e.GetMessage().Tag == DESPAWN_TAG)
        {
            DespawnPlayer(sender, e);
        }
    }

    void SpawnPlayer(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage())
        using (DarkRiftReader reader = message.GetReader())
        {
            //Each spawn packet is 5 bytes.
            //ID is one byte, each position float is 2 bytes
            if(reader.Length % 5 != 0)
            {
                Debug.LogWarning("Received malformed spawn packet");
                return;
            }

            //read until end of stream
            while(reader.Position < reader.Length)
            {
                ushort id = reader.ReadUInt16();

                //1.07 appears to be default y value
                Vector3 position = new Vector3(reader.ReadSingle(), 1.07f, reader.ReadSingle());

                if(id != client.ID)
                {
                    GameObject obj = Instantiate(networkPrefab, position, Quaternion.identity) as GameObject;
                    networkPlayerManager.Add(id, obj.GetComponent<PlayerObject>());
                }
            }
        }
    }

    void DespawnPlayer(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage())
        using (DarkRiftReader reader = message.GetReader())
            networkPlayerManager.DestroyPlayer(reader.ReadUInt16());
    }
}
