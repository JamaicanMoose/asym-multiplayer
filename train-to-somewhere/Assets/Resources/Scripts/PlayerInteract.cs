using UnityEngine;
using UnityEngine.Assertions;

public class PlayerInteract : MonoBehaviour
{
    public float interactDistance = 1.0f;
    public float groundY = 2.94f;

    private Interactable inter = null;

    private float time;

    InteractionVolume interactionVolume;

    private void Awake()
    {
        interactionVolume = gameObject.transform.Find("Interaction Volume").GetComponent<InteractionVolume>();
        Assert.IsNotNull(interactionVolume);
    }

    // Update is called once per frame
    void Update()
    {
        // If we are currently holding an interactable, 
        // we should always be interacting with that interactable.
        Pickup holding = gameObject.GetComponentInChildren<Pickup>();
        if (holding != null)
        {
            inter = holding.interactable;
            inter.interactingPlayerTransform = transform;
        }

        // If the interactable we are currently interacting with is no longer in the volume then abort.
        else if (inter != null && !interactionVolume.insideInteractionVolume.Exists(g => g.GetComponent<Interactable>() == inter))
        {
            inter.AbortUse();
            inter = null;
        }

        foreach (GameObject g in interactionVolume.insideInteractionVolume)
        {
            if (inter != null)
                break;
            if (g.GetComponent<Interactable>().interactingPlayerTransform != null)
                continue;
            inter = g.GetComponent<Interactable>();
        }


        //if the interactable we're currently interacting with leaves the interaction volume we should abort

        if (inter)
        {
            // Code from https://forum.unity.com/threads/solved-hold-button-for-3-seconds.451812/
            // Start the hold timer
            if (Input.GetButtonDown("Fire1") && !inter.inUse)
            {
                time = Time.time;
                inter.interactingPlayerTransform = transform;
                inter.StartUse();
            }
            else if (Input.GetButton("Fire1"))
            {
                if (Time.time - time > inter.holdTime)
                {
                    time = float.PositiveInfinity;
                    inter.AfterUse();
                    inter = null;
                }
                else
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
                }
                inter = null;
            }
        }
    }
}
