using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift.Server.Unity;

namespace TTS
{
    public class TrackedDataSerializeEventArgs : EventArgs
    {
        public List<TDataMessagePart> messages;

        public TrackedDataSerializeEventArgs(List<TDataMessagePart> msgs)
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
    bool isServer = false;

    //SERVER ONLY

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
    List<TTS.GameObjectMovementMessage> movementBuffer;
    List<TTS.GameObjectTDataMessage> trackedDataBuffer;
    Vector3 lastSyncedPosition;
    Quaternion lastSyncedRotation;

    public static ushort Get(GameObject go)
    {
        TTSID c = go.GetComponent<TTSID>();
        return (c == null ? (ushort)0 : c.id);
    }

    public static ushort Get(Transform t) { return Get(t.gameObject); }

    public void Init()
    {
        if (id == 0)
            id = GameObject.FindGameObjectWithTag("Network").GetComponent<TTSIDCounter>().GenerateID();

        lastSyncedPosition = transform.localPosition;
        lastSyncedRotation = transform.localRotation;
    }

    private void Start()
    {
       
        isServer = GameObject.FindGameObjectWithTag("Network")
            .GetComponent<XmlUnityServer>() != null;
        if (isServer)
        {
            Init();
            TTS.ObjectSync os = GameObject.FindGameObjectWithTag("Network")
                .GetComponent<TTS.ObjectSync>();
            movementBuffer = os.movementBuffer;
            trackedDataBuffer = os.trackedDataBuffer;
            lastSyncedPosition = transform.localPosition;
            lastSyncedRotation = transform.localRotation;
        }
        else
        {
            Destroy(GetComponent<Rigidbody>());
            if(GetComponent<BoxCollider>() != null)
            {
                Destroy(GetComponent<BoxCollider>());
            }
        }
    }

    public void Remove()
    {
        GameObject net = GameObject.FindGameObjectWithTag("Network");
        net.GetComponent<TTSIDMap>().removeTransform(id);
        if (net.GetComponent<XmlUnityServer>() != null)
        {
            net.GetComponent<TTSIDCounter>().ReleaseID(id);
            net.GetComponent<TTS.ObjectSync>().removeBuffer.Add(new TTS.GameObjectRemoveMessage(id));
        }
        Destroy(gameObject);
    }

    public void Reparent(ushort newParentID)
    {
        GameObject net = GameObject.FindGameObjectWithTag("Network");
        if (net.GetComponent<XmlUnityServer>() != null)
        {
            transform.parent = net.GetComponent<TTSIDMap>().getTransform(newParentID);
            net.GetComponent<TTS.ObjectSync>().trackedDataBuffer.Add(new TTS.GameObjectTDataMessage(transform, new List<TTS.TDataMessagePart>()));
        }
    }

    public bool ShouldSyncMovement()
    {
        return (Vector3.Distance(transform.localPosition, lastSyncedPosition) >= syncDistanceTrig) ||
               (Quaternion.Angle(lastSyncedRotation, transform.localRotation) >= syncRotationTrig);
    }

    public bool ShouldSyncTData()
    {
        return trackedDataAvailable;
    }

    private void Update()
    {
        if (isServer)
        {
            if (ShouldSyncMovement())
            {
                movementBuffer.Add(new TTS.GameObjectMovementMessage(transform));
                lastSyncedPosition = transform.localPosition;
                lastSyncedRotation = transform.localRotation;
            }
            if (ShouldSyncTData())
            {
                trackedDataBuffer.Add(new TTS.GameObjectTDataMessage(transform, SerializeData()));
            }
        }
    }

    protected virtual void OnSerializeData(TTS.TrackedDataSerializeEventArgs e)
    {
        trackedDataSerialize?.Invoke(this, e);
        trackedDataAvailable = false;
    }

    public List<TTS.TDataMessagePart> SerializeData()
    {
        List<TTS.TDataMessagePart> buffer = new List<TTS.TDataMessagePart>();
        OnSerializeData(new TTS.TrackedDataSerializeEventArgs(buffer));
        return buffer;
    }
}
