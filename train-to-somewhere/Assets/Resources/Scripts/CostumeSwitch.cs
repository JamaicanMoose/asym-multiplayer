using System.Collections;
using System.Collections.Generic;
using DarkRift;
using UnityEngine;

namespace TTS
{
    public class DoorAnimTrigger : TDataMessagePart
    {

        bool Value;


        public DoorAnimTrigger() { }

        public DoorAnimTrigger(bool value)
        {
            this.Value = value;
        }

        public override void Serialize(SerializeEvent e)
        {
            e.Writer.Write(Value);
        }

        public override void Deserialize(DeserializeEvent e)
        {
            Value = e.Reader.ReadBoolean();
        }

        public override void Load(Transform target)
        {
            target.GetComponent<CostumeSwitch>().SetBool(Value);
        }

    }
}

public class CostumeSwitch : MonoBehaviour
{
    public string JobTag;

    private bool isServer;

    Animator doorAnimator;
    bool occupied= false;

    public GameObject changeSystem;

    private void Awake()
    {
        isServer = GameObject.FindGameObjectWithTag("Network")
           .GetComponent<DarkRift.Server.Unity.XmlUnityServer>() != null;

  
        doorAnimator = GetComponentInChildren<Animator>();

        if (isServer)
        {
            GetComponent<TTSID>().trackedDataSerialize += TrackedDataHandler;

        }

    }

    void TrackedDataHandler(object sender, TTS.TrackedDataSerializeEventArgs e)
    {
            TTS.DoorAnimTrigger m = new TTS.DoorAnimTrigger(occupied);
            e.messages.Add(m);

    }

    private void OnTriggerEnter(Collider other)
    {
        if(isServer)
        {
            if (other.CompareTag("Player"))
            {
                other.GetComponent<TTSNetworkedPlayer>().SetJob(JobTag);
                occupied = true;
                SetBool(true);
                GetComponent<TTSID>().trackedDataAvailable = true;

                foreach (ParticleSystem pS in changeSystem.GetComponentsInChildren<ParticleSystem>())
                {
                    pS.Play();
                }

            }
        }
    
    }

    private void OnTriggerExit(Collider other)
    {
        if (isServer)
        {
            if (other.CompareTag("Player"))
            {
                occupied = false;
                SetBool(false);
                GetComponent<TTSID>().trackedDataAvailable = true;
            }
        }

    }

    public void SetBool(bool value)
    {
        doorAnimator.SetBool("Occupied", value);
    }
}
