using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;

namespace TTS
{
    public class NetworkedWheelMessage : TDataMessagePart
    {
        public bool frontBroken;
        public bool backBroken;

        public NetworkedWheelMessage() { }

        public NetworkedWheelMessage(bool frontBroken, bool backBroken)
        {
            this.frontBroken = frontBroken;
            this.backBroken = backBroken;
        }

        public override void Serialize(SerializeEvent e)
        {
            e.Writer.Write(frontBroken);
            e.Writer.Write(backBroken);
        }

        public override void Deserialize(DeserializeEvent e)
        {
            frontBroken = e.Reader.ReadBoolean();
            backBroken = e.Reader.ReadBoolean();
        }

        public override void Load(Transform target)
        {
            target.GetComponent<WheelBreak>().frontBroken = frontBroken;
            target.GetComponent<WheelBreak>().backBroken = backBroken;
        }
    }
}

public class WheelBreak : MonoBehaviour
{
    public float slowTimestep = 2.5f;
    public float accelerationDrop = 0.5f;
    public static float dragDelta = .05f;

    public bool frontBroken = false;
    public bool backBroken = false;

    public GameObject sparkPrefab;

    private bool trackedDataAvailable = false;
    public TTSTrainController tc;
    
    private void Awake()
    {
        if (GameObject.FindGameObjectWithTag("Network").GetComponent<DarkRift.Server.Unity.XmlUnityServer>() != null)
        {
            tc = GameObject.FindGameObjectWithTag("Train").GetComponent<TTSTrainController>();

            GetComponent<TTSID>().trackedDataSerialize += TrackedDataHandler;
        }
    }

    void TrackedDataHandler(object sender, TTS.TrackedDataSerializeEventArgs e)
    {
        if (trackedDataAvailable)
        {
            TTS.NetworkedWheelMessage m = new TTS.NetworkedWheelMessage(frontBroken, backBroken);
            e.messages.Add(m);
            trackedDataAvailable = false;
        }
    }

    void SyncState()
    {
        trackedDataAvailable = true;
        GetComponent<TTSID>().trackedDataAvailable = true;
    }

    public void Break(bool front)
    {
        if (front)
        {
            frontBroken = true;
        } else
        {
            backBroken = true;
        }

        // Add extra drag to train
        tc.AddDrag(dragDelta);

        // Spawn interactable w/ particles
        GameObject sparks = Instantiate(sparkPrefab, GameObject.FindGameObjectWithTag("Train").transform);
        sparks.transform.position = transform.position + sparks.transform.localPosition;
        if (!front)
        {
            sparks.transform.position += new Vector3(0, 0, 7.3f);
        }
        WheelInteract wi = sparks.GetComponent<WheelInteract>();
        wi.car = gameObject.GetComponent<WheelBreak>();
        wi.isFront = front;

        sparks.GetComponent<TTSID>().Init();
        TTS.GameObjectInitMessage initMessage = new TTS.GameObjectInitMessage(sparks);
        TTS.ObjectSync os = GameObject.FindGameObjectWithTag("Network").GetComponent<TTS.ObjectSync>();
        os.initBuffer.Add(initMessage);

        SyncState();
    }
}
