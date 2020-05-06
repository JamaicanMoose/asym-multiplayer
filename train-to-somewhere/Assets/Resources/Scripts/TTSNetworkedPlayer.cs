﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;

namespace TTS {
    public class NetworkedPlayerMessage : TDataMessagePart
    {
        float foodLevel;

        public NetworkedPlayerMessage() { }

        public NetworkedPlayerMessage(float foodLevel)
        {
            this.foodLevel = foodLevel;
        }

        public override void Serialize(SerializeEvent e)
        {
            e.Writer.Write(foodLevel);
        }

        public override void Deserialize(DeserializeEvent e)
        {
            foodLevel = e.Reader.ReadSingle();
        }

        public override void Load(Transform target)
        {
            target.GetComponent<TTSNetworkedPlayer>().foodLevel = foodLevel;
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

    public Transform currentTrainCar;

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
        UpdateCurrentCar();
    }

    // Update is called once per frame
    void Update()
    {
        if (isServer)
        {
            HandleInteractions();
            HandleMovement();
        }
        UpdateCurrentCar();
    }

    void UpdateCurrentCar()
    {
        Ray down = new Ray(transform.position, Vector3.down);
        RaycastHit hit;
        if (Physics.Raycast(down, out hit))
            if (hit.collider.name == "Floor")
            {
                currentTrainCar = hit.collider.transform.parent.parent;
                transform.parent = currentTrainCar;
            }
    }

    void TrackedDataHandler(object sender, TTS.TrackedDataSerializeEventArgs e)
    {
        if (trackedDataAvailable)
        {
            TTS.NetworkedPlayerMessage m = new TTS.NetworkedPlayerMessage(foodLevel);
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
                    interactObj.StartUse(transform);
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
                    heldPickup.Hold(transform);
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
            if (foodLevel >= costPerDash)
            {
                dashing = true;
                StartCoroutine(DashTimer());
            }
            else
            {
                GetComponentInChildren<MeshRenderer>().material.color = Color.red;
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

    public void EatFood(float foodValue)
    {
        foodLevel += foodValue;
        trackedDataAvailable = true;
        GetComponent<TTSID>().trackedDataAvailable = true;

        if(foodLevel > dashFoodCost)
        {
            GetComponentInChildren<MeshRenderer>().material.color = defaultColor;
        }
    }


}
