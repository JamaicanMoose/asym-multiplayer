using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift.Client;
using DarkRift.Client.Unity;
using DarkRift;

/*
 * TTSClient Unity Script
 * - Add script to same GameObject as DR2 UnityClient & TTSIDMap
 */

public class TTSClient : TTSGeneric
{
    [HideInInspector]
    public ushort localPlayerId;

    TTSIDMap idMap;

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
        idMap = GetComponent<TTSIDMap>();
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
            case TTSMessage.GAME_OBJECT_SYNC:
                SyncObjects(e);
                break;
            default:
                break;
        }
    }

    void SyncObjects(MessageReceivedEventArgs e)
    {
        using (Message m = e.GetMessage() as Message)
        {
            using (DarkRiftReader r = m.GetReader())
            {
                uint numObjects = r.ReadUInt32();
                for (int i = 0; i < numObjects; i++)
                {
                    TTSGameObjectSyncMessage syncData = r.ReadSerializable<TTSGameObjectSyncMessage>();
                    Transform toSync = idMap.getTransform(syncData.ttsid);
                    syncData.Load(toSync);
                }
            }
        }
    }

    public override Transform GetLocalPlayer()
    {
        return idMap.getTransform(localPlayerId);
    }
}
