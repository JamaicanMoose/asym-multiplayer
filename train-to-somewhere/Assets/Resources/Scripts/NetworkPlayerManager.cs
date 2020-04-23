using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using UnityEngine.Assertions;

public class NetworkPlayerManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The controllable player prefab.")]
    public GameObject controllablePrefab;

    [SerializeField]
    [Tooltip("The network controllable player prefab.")]
    public GameObject networkPrefab;

    UnityClient client;

    ushort SPAWN_TAG = 0;
    ushort MOVE_TAG = 1;
    ushort DESPAWN_TAG = 2;

    Dictionary<ushort, PlayerObject> networkPlayers = new Dictionary<ushort, PlayerObject>();

    List<PlayerInfo> InitialPlayerInfo = new List<PlayerInfo>();

    private NetworkObjectManager objectManager;

  

    public void DestroyPlayer(ushort id)
    {
        PlayerObject obj = networkPlayers[id];
        Destroy(obj.gameObject);
        networkPlayers.Remove(id);
    }

    private void Awake()
    {
        objectManager = gameObject.GetComponent<NetworkObjectManager>();
        Assert.IsNotNull(objectManager);

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
                Debug.Log("Received movement message");
                using (DarkRiftReader reader = message.GetReader())
                {
                    ushort id = reader.ReadUInt16();
                    ushort parentID = reader.ReadUInt16();
                    Vector3 newPosition = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    Vector3 newRotation = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    if (networkPlayers.ContainsKey(id))
                    {
                        PlayerObject currentPlayer = networkPlayers[id];
                        if(objectManager.cars.ContainsKey(parentID))
                        {
                            Transform parentTransform = objectManager.cars[parentID];
                            currentPlayer.transform.parent = parentTransform;
                            currentPlayer.SetRotation(newRotation);
                            currentPlayer.SetMovePosition(newPosition);
                        }
                        else
                        {
                            Debug.LogError("Given parentID not found: " + parentID);
                        }                   
                                   
                    }
                        
                }
            }
            else if (e.GetMessage().Tag == SPAWN_TAG)
            {
             
                InitializePlayers(sender, e);
            }
            else if (e.GetMessage().Tag == DESPAWN_TAG)
            {         
                using (DarkRiftReader reader = message.GetReader())
                    DestroyPlayer(reader.ReadUInt16());
            }
        }
    }

    void InitializePlayers(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage())
        using (DarkRiftReader reader = message.GetReader())
        {
            //Each spawn packet is 14 bytes.
            //ID is one byte, each position float is 2 bytes
            if (reader.Length % 14 != 0)
            {
                Debug.LogWarning("Received malformed spawn packet");
                return;
            }

            //read until end of stream
            while (reader.Position < reader.Length)
            {
                ushort id = reader.ReadUInt16();
                ushort parentCarID = reader.ReadUInt16();


                Vector3 position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                Vector3 rotation =new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                PlayerInfo newPlayer =  new PlayerInfo(id, parentCarID, position, rotation);
                InitialPlayerInfo.Add(newPlayer);
           
            }
        }
        objectManager.InitializedPlayers = true;
      
    }

    public void SpawnPlayers()
    {
        foreach(PlayerInfo player in InitialPlayerInfo)
        {
            if(player.ID == client.ID)
            {
                Debug.Log("Spawn local player");
                Transform parentTransform = objectManager.train.transform;
                if (objectManager.cars.ContainsKey(player.parentCarID))
                {
                    parentTransform = objectManager.cars[player.parentCarID];

                }
                else
                {
                    Debug.LogError("Player parent ID does not match any car ID");
                }
                GameObject obj = Instantiate(controllablePrefab, parentTransform.TransformPoint(player.localPosition), Quaternion.Euler(player.eulerRotation.x, player.eulerRotation.y, player.eulerRotation.z), parentTransform) as GameObject;
                obj.GetComponent<LocalPlayerController>().parentCarID = player.parentCarID;
                foreach(NetworkObject nObj in objectManager.objects.Values)
                {
                    Pickup pUp;
                    if(nObj.gObj.TryGetComponent<Pickup>(out pUp))
                    {
                        pUp.playerController = obj.GetComponent<LocalPlayerController>();
                    }
                }
                
            }
            else
            {
                Debug.Log("Spawn network player");
                Transform parentTransform = objectManager.train.transform;
                if (objectManager.cars.ContainsKey(player.parentCarID))
                {
                    parentTransform = objectManager.cars[player.parentCarID];

                }
                else
                {
                    Debug.LogError("Player parent ID does not match any car ID");
                }
                GameObject obj = Instantiate(networkPrefab, parentTransform.TransformPoint(player.localPosition), Quaternion.Euler(player.eulerRotation.x, player.eulerRotation.y, player.eulerRotation.z), parentTransform) as GameObject;
                            
                networkPlayers.Add(player.ID, obj.GetComponent<PlayerObject>());
            }
        }

        objectManager.SpawnedPlayers = true;
    }


}

public class PlayerInfo
{
    public ushort ID;
    public ushort parentCarID;

    public Vector3 localPosition;
    public Vector3 eulerRotation;

    public PlayerInfo(ushort id, ushort parentID, Vector3 pos, Vector3 rot)
    {
        ID = id;
        parentCarID = parentID;
        localPosition = pos;
        eulerRotation = rot;
    }
}
