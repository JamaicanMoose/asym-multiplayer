using System.Collections;
using System.Collections.Generic;
using DarkRift;
using UnityEngine;


namespace TTS
{
    public class PlayerAnimationMessage : TDataMessagePart
    {
   
        ushort Parameter;
        bool Value;

        public PlayerAnimationMessage() { }

        public PlayerAnimationMessage( ushort param, bool value)
        {
    
            this.Parameter = param;
            this.Value = value;

        }

        public override void Serialize(SerializeEvent e)
        {
       
            e.Writer.Write(Parameter);
            e.Writer.Write(Value);


        }

        public override void Deserialize(DeserializeEvent e)
        {
            Parameter = e.Reader.ReadUInt16();
            Value = e.Reader.ReadBoolean();
        }

        public override void Load(Transform target)
        {           
            target.GetComponent<TTSPlayerAnimator>().SetBool(Parameter, Value);
        }
    }
}
public class TTSPlayerAnimator : MonoBehaviour
{

    public Animator playerAnim;


    private float WalkSpeed = 1.0f;

    private Dictionary<string, bool> animParams = new Dictionary<string, bool>();
    private Dictionary<ushort, string> paramTags = new Dictionary<ushort, string>();
    
    private bool isServer = false;

    private List<ushort> changes = new List<ushort>();


    private void Awake()
    {
        animParams.Add("Walk", false);
        animParams.Add("WalkTired", false);
        animParams.Add("isTired", false);
        animParams.Add("isHolding", false);
        animParams.Add("Eat", false);
        animParams.Add("Jump", false);
        animParams.Add("Cook", false);
        animParams.Add("Fix", false);
        animParams.Add("Shovel", false);
        animParams.Add("isCannon", false);
        animParams.Add("isShooting", false);
        animParams.Add("Change", false);
        animParams.Add("Lose", false);
        animParams.Add("Win", false);

        paramTags.Add(1, "Walk");
        paramTags.Add(2, "WalkTired");
        paramTags.Add(3, "isTired");
        paramTags.Add(4, "isHolding");
        paramTags.Add(5, "Eat");
        paramTags.Add(6, "Jump");
        paramTags.Add(7, "Cook");
        paramTags.Add(8, "Fix");
        paramTags.Add(9, "Shovel");
        paramTags.Add(10, "isCannon");
        paramTags.Add(11, "isShooting");
        paramTags.Add(12, "Change");
        paramTags.Add(13, "Lose");
        paramTags.Add(14, "Win");



        isServer = GameObject.FindGameObjectWithTag("Network")
            .GetComponent<DarkRift.Server.Unity.XmlUnityServer>() != null;

        if(isServer)
        {
            GetComponent<TTSID>().trackedDataSerialize += TrackedDataHandler;

        }
    }


    void TrackedDataHandler(object sender, TTS.TrackedDataSerializeEventArgs e)
    {
       
            while(changes.Count > 0)
            {
                TTS.PlayerAnimationMessage m = new TTS.PlayerAnimationMessage(changes[0], animParams[paramTags[changes[0]]]);
                e.messages.Add(m);
                changes.RemoveAt(0);
            }         
          
        
    }


    public void SetBool(ushort paramTag, bool value)
    {
        if (animParams[paramTags[paramTag]] != value)
        {
            animParams[paramTags[paramTag]] = value;
            playerAnim.SetBool(paramTags[paramTag], value);
            
            if (isServer)
            {
                changes.Add(paramTag);
                GetComponent<TTSID>().trackedDataAvailable = true;           
            }
        }
    }
    
    public void SetWalkSpeed(float value)
    {
        if (WalkSpeed != value)
        {
            WalkSpeed = value;
            playerAnim.SetFloat("WalkSpeed", WalkSpeed);

            if (isServer)
            {
            }
        }
    }    



}
