using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;

namespace TTS {
    public class NetworkedPlayerMessage : TDataMessagePart
    {
        float foodLevel;
        string jobTag;
        public NetworkedPlayerMessage() { }

        public NetworkedPlayerMessage(float foodLevel, string jobTag)
        {
            this.foodLevel = foodLevel;
            this.jobTag = jobTag;
        }

        public override void Serialize(SerializeEvent e)
        {
            e.Writer.Write(foodLevel);
            e.Writer.Write(jobTag);
        }

        public override void Deserialize(DeserializeEvent e)
        {
            foodLevel = e.Reader.ReadSingle();
            jobTag = e.Reader.ReadString();
        }

        public override void Load(Transform target)
        {
            target.GetComponent<TTSNetworkedPlayer>().foodLevel = foodLevel;
            target.GetComponent<TTSNetworkedPlayer>().SetJob(jobTag);
        }
    }
}

public class TTSNetworkedPlayer : MonoBehaviour
{
    private Rigidbody rb;
    private bool trackedDataAvailable = false;

    public float moveSpeed = 2.5f;
    public float dashSpeed = 20f;
    public float dashTime = .3f;

    bool prevFire1ButtonDown = false;
    public bool fire1ButtonDown = false;

    bool prevFire2ButtonDown = false;
    public bool fire2ButtonDown = false;

    bool prevdashButtonDown = false;
    bool dashButtonDown = false;

    Vector3 currentMoveVector = Vector3.zero;

    bool dashing = false;

    public float dashFoodCost = .1f;
    public float foodLevel = 100f;
    public Color defaultColor;

    public Vector3 lastSyncPostion;
    private Vector3 lastMoveVector;

    private PickupVolume pVolume;
    private InteractVolume iVolume;

    private PickupGeneric heldPickup = null;
    private InteractGeneric interactObj = null;

    public GameObject hatParent;
    public GameObject toolParent;

    public string JobTag = "Engineer";

    public TTSPlayerAnimator playerAnim;

    List<GameObject> hats = new List<GameObject>();
    List<GameObject> tools = new List<GameObject>();

    bool isServer;

    private void Awake()
    {
        isServer = GameObject.FindGameObjectWithTag("Network")
            .GetComponent<DarkRift.Server.Unity.XmlUnityServer>() != null;
        if (isServer)
        {
            lastSyncPostion = transform.localPosition;
            lastMoveVector = Vector3.zero;
            rb = GetComponent<Rigidbody>();
            pVolume = GetComponentInChildren<PickupVolume>();
            iVolume = GetComponentInChildren<InteractVolume>();
            GetComponent<TTSID>().trackedDataSerialize += TrackedDataHandler;
            defaultColor = GetComponentInChildren<MeshRenderer>().material.color;
        }
      

        for(int i = 0; i < hatParent.transform.childCount; i++)
        {
            if(!hatParent.transform.GetChild(i).CompareTag("Hair"))
            {
                hats.Add(hatParent.transform.GetChild(i).gameObject);
            }
        }

        for (int i = 0; i < toolParent.transform.childCount; i++)
        {
           
            tools.Add(toolParent.transform.GetChild(i).gameObject);
            
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (isServer)
        {
            HandleInteractions();
            HandleMovement();
            HandleAnimations();
        }
      
    }

    public Transform GetCurrentCar()
    {
        Ray down = new Ray(transform.position, Vector3.down);
        RaycastHit hit;
        if (Physics.Raycast(down, out hit))
            if (hit.collider.name == "Floor")
            {
                Transform currentTrainCar = hit.collider.transform.parent.parent;
                return currentTrainCar;
            }

        return null;
    }

    void TrackedDataHandler(object sender, TTS.TrackedDataSerializeEventArgs e)
    {
        if (trackedDataAvailable)
        {
            TTS.NetworkedPlayerMessage m = new TTS.NetworkedPlayerMessage(foodLevel, JobTag);
            e.messages.Add(m);
            trackedDataAvailable = false;
        }
    }

    // INTERACTIONS

    void HandleInteractions()
    {
        bool onFire1ButtonDown = fire1ButtonDown && !prevFire1ButtonDown;
        bool onFire1ButtonUp = !fire1ButtonDown && prevFire1ButtonDown;

        bool onFire2ButtonDown = fire2ButtonDown && !prevFire2ButtonDown;
        bool onFire2ButtonUp = !fire2ButtonDown && prevFire2ButtonDown;

        if(onFire1ButtonDown)
        {
            if(heldPickup != null)
            {
                heldPickup.Interact();
                heldPickup = null;
            }
            else
            {
                if(iVolume.potentialInteracts.Count > 0)
                {
                    interactObj = iVolume.potentialInteracts[0].GetComponent<InteractGeneric>();
                    if (!interactObj.requiresCostume || interactObj.costume == JobTag)
                    {
                        interactObj.StartUse(transform);
                        iVolume.interactingObj = interactObj;
                    }
                 }
            }
        }

        if (onFire2ButtonDown)
        {
      
            if(heldPickup != null)
            {
                heldPickup.Drop();
                heldPickup = null;          
             
            }
            else
            {
                if (pVolume.potentialPickups.Count > 0)
                {                 
                    heldPickup = pVolume.potentialPickups[0].GetComponent<PickupGeneric>();
                    if (!heldPickup.requiresCostume || heldPickup.costume == JobTag)
                    {
                        heldPickup.Hold(transform);
                    }
                }
            }
          
        }
        

        prevFire1ButtonDown = fire1ButtonDown;
        prevFire2ButtonDown = fire2ButtonDown;
    }

    // MOVEMENT

    void HandleMovement()
    {
        bool onDashButtonDown = dashButtonDown && !prevdashButtonDown;

        if (!dashing && onDashButtonDown && currentMoveVector != Vector3.zero)
        {
            float costPerDash = dashFoodCost * (dashTime / Time.deltaTime);
            if (foodLevel > 0)
            {
                dashing = true;
                StartCoroutine(DashTimer());
            }
            else
            {
                GetComponentInChildren<MeshRenderer>().material.color = Color.red;
                dashing = false;
                foodLevel = 0;
            }

            

        }
        if (dashing)
        {
            foodLevel -= dashFoodCost;
            trackedDataAvailable = true;
            GetComponent<TTSID>().trackedDataAvailable = true;
            rb.velocity = currentMoveVector * dashSpeed;
        }
        else
        {
            rb.velocity = currentMoveVector * moveSpeed;
        }

        prevdashButtonDown = dashButtonDown;

        lastMoveVector = currentMoveVector;

        if (transform.forward.normalized != lastMoveVector.normalized)
        {
            transform.forward = Vector3.RotateTowards(transform.forward, lastMoveVector, 10 * Time.deltaTime, 0.0f);
            Debug.DrawRay(transform.position, lastMoveVector, Color.red);
        }
    }

    void HandleAnimations()
    {
        bool moving;
        bool holding;
        bool tired;

        if (lastMoveVector != Vector3.zero)
            moving = true;
        else
            moving = false;

        if (heldPickup != null)
            holding = true;
        else
            holding = false;

        if(foodLevel <= 0)
        {
            tired = true;
        }
        else
        {
            tired = false;
        }

        if (dashing)
        {
            playerAnim.SetWalkSpeed(dashSpeed / 2);
        }
        else
        {
            playerAnim.SetWalkSpeed(1);
        }

        if (moving)
        {

            playerAnim.SetBool(1, true);
       
        }
        else
        {
            playerAnim.SetBool(1, false);

        }

        if(holding)
        {
            playerAnim.SetBool(4, true);
        }
        else
        {
            playerAnim.SetBool(4, false);
        }

        if (tired)
        {
            playerAnim.SetBool(3, true);
        }
        else
        {
            playerAnim.SetBool(3, false);
        }
    }

    IEnumerator DashTimer()
    {
        yield return new WaitForSeconds(dashTime);
        dashing = false;
    }

    public void SetMovementInput(Vector3 direction, bool dashButton)
    {
        dashButtonDown = dashButton;
        currentMoveVector = direction;
    }

    public void SetFireInput(bool fire1Down, bool prevFire1Down, bool fire2Down, bool prevFire2Down)
    {
        fire1ButtonDown = fire1Down;
        prevFire1ButtonDown = prevFire1Down;

        fire2ButtonDown = fire2Down;
        prevFire2ButtonDown = prevFire2Down;
    }

    public void EatFood(float foodValue, TTSID toRemove)
    {
        foodLevel += foodValue;
        trackedDataAvailable = true;
        GetComponent<TTSID>().trackedDataAvailable = true;

        if(foodLevel > dashFoodCost)
        {
            GetComponentInChildren<MeshRenderer>().material.color = defaultColor;
        }

        playerAnim.SetBool(5, true);
        StartCoroutine(EatAnim(toRemove));
    }

    public void SetJob(string jobTag)
    {
    
       if(JobTag != jobTag)
        {
            JobTag = jobTag;
            foreach (GameObject hat in hats)
            {
                if (hat.CompareTag(jobTag))
                {
                    hat.SetActive(true);
                }
                else
                {
                    hat.SetActive(false);
                }
            }

            foreach (GameObject tool in tools)
            {
                if (tool.CompareTag(jobTag))
                {
                    tool.SetActive(true);
                }
                else
                {
                    tool.SetActive(false);
                }
            }

            trackedDataAvailable = true;
            GetComponent<TTSID>().trackedDataAvailable = true;
            playerAnim.SetTrigger(12);
        }
        
    
    }

    IEnumerator EatAnim(TTSID toRemove)
    {
        yield return new WaitForSeconds(.8f);
        playerAnim.SetBool(5, false);
        toRemove.Remove();

    }


}
