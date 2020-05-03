using System;
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

    bool gameStarted = false;

    TTSIDMap idMap;

    UnityClient client;

    Transform mainCameraTransform;
    bool fire1 = false;
    bool fire2 = false;

    bool lastFire1 = false;
    bool lastFire2 = false;

    bool dashing = false;
    bool lastDashing = false;
    Vector3 lastMoveVector = Vector3.zero;

    private void Awake()
    {
        GetComponent<TTSGeneric>().GameStarted += OnGameStart;
    }

    void OnGameStart(object sender, EventArgs e)
    {
        gameStarted = true;
    }

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
        if (gameStarted)
            HandleInput();
    }

    void HandleInput()
    {
        Vector3 moveDirection = mainCameraTransform.right * Input.GetAxis("Horizontal") + mainCameraTransform.forward * Input.GetAxis("Vertical");
        moveDirection.y = 0f;
        moveDirection.Normalize();
        dashing = Input.GetButton("Jump");

        if (moveDirection != lastMoveVector || dashing != lastDashing)
        {
            using (DarkRiftWriter moveInputWriter = DarkRiftWriter.Create())
            {
                moveInputWriter.Write(new TTS.InputMovementMessage(moveDirection, dashing));
                using (Message moveInputMessage = Message.Create((ushort)TTS.MessageType.INPUT_MOVEMENT, moveInputWriter))
                    client.SendMessage(moveInputMessage, SendMode.Unreliable);
            }
        }

        lastMoveVector = moveDirection;
        lastDashing = dashing;

        fire1 = Input.GetButton("Fire1");
        fire2 = Input.GetButton("Fire2");

        if (fire1 != lastFire1 || fire2 != lastFire2)
        {
            using (DarkRiftWriter interactInputWriter = DarkRiftWriter.Create())
            {
                interactInputWriter.Write(new TTS.InputInteractMessage(fire1, fire2));
                using (Message interactInputMessage = Message.Create((ushort)TTS.MessageType.INPUT_INTERACT, interactInputWriter))
                    client.SendMessage(interactInputMessage, SendMode.Reliable);
            }
        }

        lastFire1 = fire1;
        lastFire2 = fire2;
    }

    void ClientMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        switch((TTS.MessageType)e.GetMessage().Tag)
        {
            case TTS.MessageType.GAME_OBJECT_MOVE:
                using (Message m = e.GetMessage() as Message)
                using (DarkRiftReader r = m.GetReader())
                {
                    uint numObjects = r.ReadUInt32();
                    for (int i = 0; i < numObjects; i++)
                    {
                        TTS.GameObjectMovementMessage syncData = r.ReadSerializable<TTS.GameObjectMovementMessage>();
                        Transform toSync = idMap.getTransform(syncData.ttsid);
                        syncData.Load(toSync);
                    }
                }
                break;
            case TTS.MessageType.GAME_OBJECT_TDATA:
                using (Message m = e.GetMessage() as Message)
                using (DarkRiftReader r = m.GetReader())
                {
                    uint numObjects = r.ReadUInt32();
                    for (int i = 0; i < numObjects; i++)
                    {
                        TTS.GameObjectTDataMessage syncData = r.ReadSerializable<TTS.GameObjectTDataMessage>();
                        Debug.Log($"TDATA {syncData.ttsid} {syncData.parent}");
                        Transform toSync = idMap.getTransform(syncData.ttsid);
                        syncData.Load(toSync);
                    }
                }
                break;
        }
    }

    public override Transform GetLocalPlayer()
    {
        return idMap.getTransform(localPlayerId);
    }
}
