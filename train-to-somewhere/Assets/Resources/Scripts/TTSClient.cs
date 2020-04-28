using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift.Client;
using DarkRift.Client.Unity;
using DarkRift;

public class TTSClient : TTSGeneric
{
    [HideInInspector]
    public ushort localPlayerId;

    Dictionary<ushort, Transform> idMap;

    [SerializeField]
    [Tooltip("The local player prefab.")]
    public GameObject controllablePlayerPrefab;

    UnityClient client;

    Transform mainCameraTransform;
    bool dashing = false;
    bool lastDashing = false;
    Vector3 lastMoveVector = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        idMap = GameObject.FindGameObjectWithTag("Network").GetComponent<TTSIDMap>().idMap;
        client = GetComponent<UnityClient>();
        client.Client.MessageReceived += ClientMessageReceived;
        mainCameraTransform = GameObject.FindGameObjectWithTag("MainCamera").transform;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 moveDirection = mainCameraTransform.right * Input.GetAxis("Horizontal") + mainCameraTransform.forward * Input.GetAxis("Vertical");

        moveDirection.y = 0f;
        moveDirection.Normalize();

        dashing = Input.GetKey(KeyCode.Space);

  
        if (moveDirection != lastMoveVector || dashing != lastDashing)
        {
           using (DarkRiftWriter moveInputWriter = DarkRiftWriter.Create())
           {

                moveInputWriter.Write(new TTSInputMessage(moveDirection, dashing));
                using (Message moveInputMessage = Message.Create(TTSMessage.MOVEMENT_INPUT, moveInputWriter))
                {
                    client.SendMessage(moveInputMessage, SendMode.Unreliable);
                }
           }

        }

        lastMoveVector = moveDirection;
        lastDashing = dashing;

    }

    void ClientMessageReceived(object sender, MessageReceivedEventArgs e)
    {

        switch(e.GetMessage().Tag)
        {
            case TTSMessage.PLAYER_SYNC:
                SyncPlayer(e);
                break;
            default:
                break;
        }
    }

    void SyncPlayer(MessageReceivedEventArgs e)
    {
          
        TTSGameObjectSyncMessage syncData;
        using (Message m = e.GetMessage() as Message)
        {
            using (DarkRiftReader r = m.GetReader())
            {
                syncData = r.ReadSerializable<TTSGameObjectSyncMessage>();
            }
        }
        ushort playerID = syncData.ttsid;
        Vector3 localPosition = syncData.position;
        Quaternion localRotation = syncData.rotation;

        if(idMap.ContainsKey(playerID))
        {
            Transform toSync = idMap[playerID];
            toSync.localPosition = localPosition;
            toSync.localRotation = localRotation;  
        }

    }

    public override Transform GetLocalPlayer()
    {
        return idMap[localPlayerId];
    }
}
