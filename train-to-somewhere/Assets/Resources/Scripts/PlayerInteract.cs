using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Animations;

public class PlayerInteract : MonoBehaviour
{
    public float interactDistance = 1.0f;
    public float groundY = 2.94f;

    private Interactable inter = null;

    private float holdTime = float.PositiveInfinity;
    private float time;
    private int interLayer;

    InteractionVolume interactionVolume;

    private void Awake()
    {
        interLayer = LayerMask.GetMask("InteractableLayer");
        interactionVolume = gameObject.transform.Find("Interaction Volume").GetComponent<InteractionVolume>();
        Assert.IsNotNull(interactionVolume);
    }

    // Update is called once per frame
    void Update()
    {

        Pickup holding = gameObject.GetComponentInChildren<Pickup>();
        // If we are currently holding an object, interacting should instead drop the object.
        if (holding != null && Input.GetButtonDown("Fire1"))
        {
            holding.transform.parent = transform.parent;
            gameObject.GetComponent<PositionConstraint>().constraintActive = false;
            gameObject.GetComponent<PositionConstraint>().RemoveSource(0);
            return;
        }

        inter = null;
        foreach (GameObject g in interactionVolume.insideInteractionVolume)
        {
            inter = g.GetComponent<Interactable>();
            if (inter != null)
                break;
        }

        if (inter)
        {
            // Code from https://forum.unity.com/threads/solved-hold-button-for-3-seconds.451812/
            // Start the hold timer
            if (Input.GetButtonDown("Fire1") && !inter.inUse)
            {
                time = Time.time;
                inter.inUse = true;
                inter.StartUse();
            }
            else if (Input.GetButton("Fire1"))
            {
                if (Time.time - time > inter.holdTime)
                {
                    time = float.PositiveInfinity;
                    
                    inter.AfterUse();
                    inter.inUse = false;
                } else
                {
                    if (inter.inUse)
                    {
                        inter.DuringUse();
                    }
                }
            // Cancel the hold timer if let go
            }
            else
            {
                time = float.PositiveInfinity;
                if (inter.inUse)
                {
                    inter.AbortUse();
                    inter.inUse = false;
                }
            }
        }
    }
}
