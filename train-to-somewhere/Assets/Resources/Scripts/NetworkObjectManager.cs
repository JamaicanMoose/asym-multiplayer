using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using DarkRift.Client.Unity;
using DarkRift.Client;
using DarkRift;
using UnityEngine;
using UnityEngine.Assertions;



public class NetworkObjectManager : MonoBehaviour
{
    //Tags for identifying incoming messages
    const ushort TRAIN_TAG = 3;
    const ushort OBJECT_TAG = 4;
    const ushort PICKUP_TAG = 5;
    const ushort USE_TAG = 6;
    const ushort NUM_PLAYERS_TAG = 7;
    const ushort LAUNCH_TAG = 8;

    //Tags for the use state of interactable object use messages
    const ushort START_USE = 1;
    const ushort DURING_USE = 2;
    const ushort AFTER_USE = 3;
    const ushort ABORT_USE = 4;

    //Tags for which type of car to spawn
    const ushort ENGINE = 0;
    const ushort DEFAULT_CAR = 1;
    const ushort CABOOSE = 2;

    
    UnityClient client;

    [SerializeField]
    [Tooltip("The train engine prefab.")]
    public GameObject enginePrefab;

    [SerializeField]
    [Tooltip("The train car prefab.")]
    public GameObject carPrefab;

    [SerializeField]
    [Tooltip("The caboose prefab.")]
    public GameObject caboosePrefab;

    [SerializeField]
    [Tooltip("The testPickup prefab")]
    public GameObject testPickupPrefab;

    [SerializeField]
    [Tooltip("The testInteractable prefab.")]
    public GameObject testInteractablePrefab;

    [SerializeField]
    [Tooltip("The Train object in the scene")]
    public GameObject train;

    [SerializeField]
    [Tooltip("The NetworkPlayerManager prefab.")]
    public NetworkPlayerManager playerManager;

  
    public Text numPlayerText;
    public Canvas blockCanvas;
    public float MoveDistance = .05f;
    //Key is cars Networktrackable.UniqueID, Value is the cars transform
    public Dictionary<ushort, Transform> cars = new Dictionary<ushort, Transform>();

    //Key is objects Networktrackable.UniqueID, Value is the gameobject
    //This is for interactable objects only
    public Dictionary<ushort, NetworkObject> objects = new Dictionary<ushort, NetworkObject>();

    bool SpawnedTrains = false;
    bool SpawnedObjects = false;
    public bool SpawnedPlayers = false;
    public bool InitializedPlayers = false;

    private void Awake()
    {
        if (enginePrefab == null)
        {
            Debug.LogError("Train engine Prefab unassigned in TrainSpawner.");
            Application.Quit();
        }

        if (carPrefab == null)
        {
            Debug.LogError("Car Prefab unassigned in TrainSpawner.");
            Application.Quit();
        }

        client = gameObject.GetComponent<UnityClient>();
        Assert.IsNotNull(client);

        client.MessageReceived += ClientMessageReceived;
    }

    private void Update()
    {
        if(SpawnedTrains && !SpawnedObjects)
        {
            SpawnObjects();
        }
        else if(SpawnedObjects && InitializedPlayers && !SpawnedPlayers)
        {
            playerManager.SpawnPlayers();
            blockCanvas.enabled = false;
            CheckObjectMotion();
        }
        else
        {
            CheckObjectMotion();
        }
    }

    void ClientMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        //Messages with TRAIN_TAG are for spawning train cars
        if (e.GetMessage().Tag == TRAIN_TAG)
        {
            using (Message message = e.GetMessage())
            using (DarkRiftReader reader = message.GetReader())
            {
                int carCount = reader.ReadInt32();
                for (int i = 0; i < carCount; i++)
                {
                    ushort carID = reader.ReadUInt16();
                    ushort carType = reader.ReadUInt16();

                    GameObject newCar;
                    switch (carType)
                    {
                        case ENGINE:
                            newCar = Instantiate(enginePrefab);
                            break;
                        case DEFAULT_CAR:
                            newCar = Instantiate(carPrefab);
                            break;
                        case CABOOSE:
                            newCar = Instantiate(caboosePrefab);
                            break;
                        default:
                            newCar = Instantiate(carPrefab);
                            break;
                    }

                    newCar.transform.parent = train.transform;
                    newCar.transform.rotation = Quaternion.identity;
                    newCar.transform.localPosition = new Vector3(0, 2.9f, -16.13f + 16.13f * i);
                    newCar.GetComponent<NetworkTrackable>().uniqueID = carID;
                    cars.Add(carID, newCar.transform);
               
                }
            }
            SpawnedTrains = true;
        }
        //Message with OBJECT_TAG are for spawning Interactable Objects
        else if (e.GetMessage().Tag == OBJECT_TAG)
        {            
            using (Message message = e.GetMessage())
            using (DarkRiftReader reader = message.GetReader())
            {
                int objectCount = reader.ReadInt32();

                for(int i = 0; i < objectCount; i++)
                {
                    ushort objID = reader.ReadUInt16();
                    ushort objType = reader.ReadUInt16();
                    Vector3 localPosition = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    Vector3 eulerRotation = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
               
                    NetworkObject netObj = new NetworkObject(objID, objType, localPosition, eulerRotation);
                    objects.Add(objID, netObj);              
                
                }           

            }         
        }
        //Messages with PICKUP_TAG contain the new position and rotation of an object with "Pickup" attached
        else if(e.GetMessage().Tag == PICKUP_TAG)
        {
            using (Message message = e.GetMessage())
            using (DarkRiftReader reader = message.GetReader())
            {
                
                ushort objID = reader.ReadUInt16();
                if (objects.ContainsKey(objID))
                {
                    GameObject obj = objects[objID].gObj;
                    Vector3 newLocalPosition = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
              
                    float angleX = reader.ReadSingle();
                    float angleY = reader.ReadSingle();
                    float angleZ = reader.ReadSingle();

                    obj.transform.localPosition = newLocalPosition;
                    obj.GetComponent<Pickup>().lastPostion = newLocalPosition;
                    Vector3 newRotation = new Vector3(angleX, angleY, angleZ);
                    obj.transform.eulerAngles = newRotation;

                    objects[objID].localPosition = newLocalPosition;
                    objects[objID].rotation = newRotation;
          
                }
                    
        
            }
                
        }
        //Messages with USE_TAG contain the new use state of an interactable object
        else if(e.GetMessage().Tag == USE_TAG)
        {
            using (Message message = e.GetMessage())
            using (DarkRiftReader reader = message.GetReader())
            {
                ushort objID = reader.ReadUInt16();
                if (objects.ContainsKey(objID))
                {
                    GameObject obj = objects[objID].gObj;
                    ushort useTag = reader.ReadUInt16();

                    switch(useTag)
                    {
                        case START_USE:
                            obj.GetComponent<Interactable>().StartUse();
                            break;
                        case DURING_USE:
                            obj.GetComponent<Interactable>().DuringUse();
                            break;
                        case AFTER_USE:
                            obj.GetComponent<Interactable>().AfterUse();
                            break;
                        case ABORT_USE:
                            obj.GetComponent<Interactable>().AbortUse();
                            break;
                    }
                }                
                else
                {
                    Debug.LogWarning("Warning, received ID does not match an object");
                }
            }
        }
        else if(e.GetMessage().Tag == NUM_PLAYERS_TAG)
        {
            using (Message message = e.GetMessage())
            using (DarkRiftReader reader = message.GetReader())
            {
                int numPlayers = reader.ReadInt32();
                numPlayerText.text = "Players: " + numPlayers.ToString();
            }
        }
    }

    public void SendMovementMessage(ushort objID)
    {
        GameObject obj = objects[objID].gObj;
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(obj.GetComponent<NetworkTrackable>().uniqueID);




            Vector3 localPosition = obj.transform.localPosition;

            writer.Write(localPosition.x);
            writer.Write(localPosition.y);
            writer.Write(localPosition.z);
            

            Vector3 angles = obj.transform.rotation.eulerAngles;
            writer.Write(angles.x);
            writer.Write(angles.y);
            writer.Write(angles.z);

            using (Message message = Message.Create(PICKUP_TAG, writer))
                client.SendMessage(message, SendMode.Unreliable);

           
        }
    }

    private void CheckObjectMotion()
    {
        foreach(NetworkObject nObj in objects.Values)
        {

            if (Vector3.Distance(nObj.localPosition, train.transform.InverseTransformPoint(nObj.gObj.transform.position)) > MoveDistance)
            {
                Debug.Log("Object moved");
                SendMovementMessage(nObj.ID);
                nObj.localPosition = train.transform.InverseTransformPoint(nObj.gObj.transform.position);
                nObj.rotation = nObj.gObj.transform.eulerAngles;
            }

        }
    }

    private void SpawnObjects()
    {
        foreach(NetworkObject nObj in objects.Values)
        {
            GameObject newObj;
            switch(nObj.ObjType)
            {
                case 0:
                    newObj = Instantiate(testInteractablePrefab, train.transform.TransformPoint(nObj.localPosition), Quaternion.Euler(nObj.rotation.x, nObj.rotation.y, nObj.rotation.z), train.transform);
                    break;
                case 1:
                    newObj = Instantiate(testPickupPrefab, train.transform.TransformPoint(nObj.localPosition), Quaternion.Euler(nObj.rotation.x, nObj.rotation.y, nObj.rotation.z), train.transform);
                    newObj.GetComponent<Pickup>().lastPostion = train.transform.InverseTransformPoint(nObj.localPosition);
                    newObj.GetComponent<Pickup>().ID = nObj.ID;
                    break;
                default:
                    newObj = Instantiate(testInteractablePrefab, train.transform.TransformPoint(nObj.localPosition), Quaternion.Euler(nObj.rotation.x, nObj.rotation.y, nObj.rotation.z), train.transform);
                    break;
            }     
   
            newObj.GetComponent<NetworkTrackable>().uniqueID = nObj.ID;
            nObj.gObj = newObj;
        }
        SpawnedObjects = true;
    }

    public void LaunchGame()
    {
        Debug.Log("LAUNCH GAME");
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            ushort yes = 1;
            writer.Write(yes);
            
            using (Message message = Message.Create(LAUNCH_TAG, writer))
                client.SendMessage(message, SendMode.Reliable);

        }
    }

}

public class NetworkObject
{
    public ushort ID;
    public ushort ObjType;  

    public Vector3 localPosition;
    public Vector3 rotation;

    public GameObject gObj;

    public NetworkObject(ushort id, ushort type,  Vector3 pos, Vector3 rot)
    {
        ID = id;
        ObjType = type;
        localPosition = pos;
        rotation = rot;        
    }
}
