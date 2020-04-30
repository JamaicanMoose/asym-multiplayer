using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift.Server.Unity;

namespace TTS
{
    public class TrackedDataSerializeEventArgs : EventArgs
    {
        public List<TTS.GODataSyncMessage> messages;

        public TrackedDataSerializeEventArgs(List<TTS.GODataSyncMessage> msgs)
        {
            messages = msgs;
        }
    }

    public delegate void TrackedDataHandler(object sender, TrackedDataSerializeEventArgs e);
}

public class TTSID : MonoBehaviour
{
    [HideInInspector]
    public ushort id = 0;

    /*
     * Components that have data that need to be serialized should add event
     * handlers to trackedDataSerialize. Handlers should have the following structure:
     * 
     * void TrackedDataHandler(object sender, TrackedDataSerializeEventArgs e) {
     *   ... build data message subclass of TTS.GODataSyncMessage
     *   e.messages.Add(message);
     * }
     * 
     * When they have data available they should set trackedDataAvailable to true;
     */
    [HideInInspector]
    public event TTS.TrackedDataHandler trackedDataSerialize;

    [HideInInspector]
    public bool trackedDataAvailable = false;


    [Tooltip("Minimum distance delta before server will sync to clients.")]
    public float syncDistanceTrig = 0.05f;
    [Tooltip("Minimum rotation angle delta before server will sync to clients.")]
    public float syncRotationTrig = 0.05f;
    bool isServer = false;
    List<TTSGameObjectSyncMessage> objectSyncBuffer;
    Vector3 lastSyncedPosition;
    Quaternion lastSyncedRotation;

    public void Init()
    {
        if (id == 0)
            id = GameObject.FindGameObjectWithTag("Network").GetComponent<TTSIDCounter>().GenerateID();
    }

    private void Start()
    {
        Init();
        isServer = GameObject.FindGameObjectWithTag("Network").GetComponent<XmlUnityServer>() != null;
        if (isServer)
        {
            objectSyncBuffer = GameObject.FindGameObjectWithTag("Network").GetComponent<TTS.ObjectSync>().buffer;
            lastSyncedPosition = transform.localPosition;
            lastSyncedRotation = transform.localRotation;
        }
    }

    public bool ShouldSync()
    {
        return (Vector3.Distance(transform.localPosition, lastSyncedPosition) >= syncDistanceTrig) ||
               (Quaternion.Angle(lastSyncedRotation, transform.localRotation) >= syncRotationTrig) ||
               trackedDataAvailable;
    }

    private void Update()
    {
        if (isServer && ShouldSync())
        {
            objectSyncBuffer.Add(new TTSGameObjectSyncMessage(id, transform, SerializeData()));
            lastSyncedPosition = transform.localPosition;
            lastSyncedRotation = transform.localRotation;
        }
    }

    protected virtual void OnSerializeData(TTS.TrackedDataSerializeEventArgs e)
    {
        trackedDataSerialize?.Invoke(this, e);
        trackedDataAvailable = false;
    }

    public List<TTS.GODataSyncMessage> SerializeData()
    {
        List<TTS.GODataSyncMessage> buffer = new List<TTS.GODataSyncMessage>();
        OnSerializeData(new TTS.TrackedDataSerializeEventArgs(buffer));
        return buffer;
    }
}
