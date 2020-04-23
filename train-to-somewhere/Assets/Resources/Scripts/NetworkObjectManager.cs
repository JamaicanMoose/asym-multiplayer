using System.Collections;
using System.Collections.Generic;
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

    //Tags for the use state of interactable object use messages
    const ushort START_USE = 1;
    const ushort DURING_USE = 2;
    const ushort AFTER_USE = 3;
    const ushort ABORT_USE = 4;

    //Tags for which type of car to spawn
    const ushort ENGINE = 0;
    const ushort DEFAULT_CAR = 1;
    const ushort CABOOSE = 2;
    const ushort KITCHEN_CAR = 3;

    
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
    [Tooltip("The kitchen car prefab.")]
    public GameObject kitchencarPrefab;

    [SerializeField]
    [Tooltip("The testPickup prefab")]
    public GameObject testPickupPrefab;

    [SerializeField]
    [Tooltip("The testInteractable prefab.")]
    public GameObject testInteractablePrefab;

    [SerializeField]
    [Tooltip("The Train object in the scene")]
    public GameObject train;

    //Key is cars Networktrackable.UniqueID, Value is the cars transform
    Dictionary<ushort, Transform> cars = new Dictionary<ushort, Transform>();

    //Key is objects Networktrackable.UniqueID, Value is the gameobject
    //This is for interactable objects only
    Dictionary<ushort, GameObject> objects = new Dictionary<ushort, GameObject>();


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
                        case KITCHEN_CAR:
                            newCar = Instantiate(kitchencarPrefab);
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
                    Debug.Log("Car type: " + carType + " ID: " + carID);
                }
            }
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
                    ushort parentCarID = reader.ReadUInt16();

                    float localX = reader.ReadSingle();
                    float localY = reader.ReadSingle();
                    float localZ = reader.ReadSingle();

                    float angleX = reader.ReadSingle();
                    float angleY = reader.ReadSingle();
                    float angleZ = reader.ReadSingle();

                    GameObject newObj;

                    switch(objType)
                    {
                        case 0:
                            newObj = Instantiate(testInteractablePrefab);
                            break;
                        case 1:
                            newObj = Instantiate(testPickupPrefab);
                            break;
                        default:
                            newObj = Instantiate(testInteractablePrefab);
                            break;
                    }

                    newObj.GetComponent<NetworkTrackable>().uniqueID = objID;
                    objects.Add(objID, newObj);
                    Transform parentCar;
                    Vector3 localPosition = new Vector3(localX, localY, localZ);

                    newObj.transform.root.eulerAngles = new Vector3(angleX, angleY, angleZ);

                    if (cars.TryGetValue(parentCarID, out parentCar))
                    {
                        newObj.transform.parent = parentCar;
                        newObj.transform.localPosition = localPosition;
                    }
                    else
                    {
                        StartCoroutine(parentAssign(parentCarID, newObj, localPosition));
                    }                
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

                       Debug.Log("Pickup Move message received");

                         GameObject obj = objects[objID];
                        obj.transform.SetParent(cars[reader.ReadUInt16()]);
                        float x = reader.ReadSingle();
                        float y = reader.ReadSingle();
                        float z = reader.ReadSingle();
                        float angleX = reader.ReadSingle();
                        float angleY = reader.ReadSingle();
                        float angleZ = reader.ReadSingle();

                        obj.transform.localPosition = new Vector3(x, y, z);
                        obj.transform.eulerAngles = new Vector3(angleX, angleY, angleZ);
                    }
                    else
                    {
                        reader.Dispose();
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
                    GameObject obj = objects[objID];
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
    }

    //This function is for assigning the parent car of a newly created object after a delay
    //This delay is because objects and cars are created simultaneously and to protect against
    //assiging an objects parent as a nonexistent transform
    IEnumerator parentAssign(ushort parentID, GameObject obj, Vector3 localPosition)
    {
        yield return new WaitForSeconds(1);
        obj.transform.parent = cars[parentID];
        obj.transform.localPosition = localPosition;
    }
}
