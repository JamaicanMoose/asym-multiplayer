using System.Collections;
using System.Collections.Generic;
using DarkRift.Client.Unity;
using DarkRift.Client;
using DarkRift;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerSpawner : MonoBehaviour
{

    ushort SPAWN_TAG = 0;
    ushort DESPAWN_TAG = 2;

    UnityClient client;

    [SerializeField]
    [Tooltip("The controllable player prefab.")]
    public GameObject controllablePrefab;

    [SerializeField]
    [Tooltip("The network controllable player prefab.")]
    public GameObject networkPrefab;

    NetworkPlayerManager networkPlayerManager;

    void Awake()
    {

        IniFile.IniWriteValue(IniFile.Sections.Train, IniFile.Keys.Address, "127.0.0.1");

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

        client = gameObject.GetComponent<UnityClient>();
        Assert.IsNotNull(client);
        networkPlayerManager = gameObject.GetComponent<NetworkPlayerManager>();
        Assert.IsNotNull(networkPlayerManager);

        //Subscribe our MessageReceived function to the client.MessageReceived event
        client.MessageReceived += MessageReceived;
    }

    void MessageReceived(object sender, MessageReceivedEventArgs e)
    {
        if(e.GetMessage().Tag == SPAWN_TAG)
        {
            Debug.Log("SPAWN MESSAGE RECEIVED");
            SpawnPlayer(sender, e);
        }
        else if(e.GetMessage().Tag == DESPAWN_TAG)
        {
            Debug.Log("DESPAWN MESSAGE RECEIVED");
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
                Vector3 position = new Vector3(reader.ReadSingle(), NetworkPlayerManager.DEFAULT_PLAYER_Y, reader.ReadSingle());

                if(id != client.ID)
                {
                    Debug.Log($"SPAWNING {id}");
                    GameObject obj = Instantiate(networkPrefab, position, Quaternion.identity) as GameObject;
                    obj.transform.parent = gameObject.transform;
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
