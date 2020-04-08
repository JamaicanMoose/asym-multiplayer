using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    public float interactDistance = 1.0f;
    public float holdTime = float.PositiveInfinity;
    public float groundY = 2.94f;
    
    private Interactable inter;

    private float time;
    private int interLayer;

    private void Awake()
    {
        interLayer = LayerMask.GetMask("InteractableLayer");
        inter = null;
    }

    // Update is called once per frame
    void Update()
    {
        // Get an object to interact with
        RaycastHit hit;
        Vector3 from = transform.position;
        from.y = groundY;
        if (Physics.Raycast(from, Vector3.forward, out hit, interactDistance, interLayer)) {
            inter = hit.collider.gameObject.GetComponent<Interactable>();
            holdTime = inter.holdTime;
        } else
        {
            inter = null;
        }

        if (inter)
        {
            // Code from https://forum.unity.com/threads/solved-hold-button-for-3-seconds.451812/
            // Start the hold timer
            if (Input.GetKeyDown("z") && !inter.inUse)
            {
                time = Time.time;
                inter.inUse = true;
            }
            else if (Input.GetKey("z"))
            {
                if (Time.time - time > holdTime)
                {
                    time = float.PositiveInfinity;
                    
                    Debug.Log("Finished!");
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
                inter.inUse = false;
            }
        }
    }
}
