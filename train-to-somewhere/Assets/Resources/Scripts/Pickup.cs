using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class Pickup : MonoBehaviour
{

    public float pickupTime = 0f;

    Transform localPlayerTransform;
    Interactable interactable;

    private void Awake()
    {
        localPlayerTransform = GameObject.FindGameObjectWithTag("LocalPlayer").transform;
        interactable = gameObject.GetComponent<Interactable>();
        if (interactable == null)
            interactable = gameObject.AddComponent<Interactable>() as Interactable;
        interactable.holdTime = pickupTime;
        //interactable.startUse.AddListener(OnInteractStart);
        //interactable.duringUse.AddListener(OnInteracting);
        interactable.afterUse.AddListener(OnInteractComplete);
        //interactable.abortUse.AddListener(OnInteractAbort);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnInteractComplete()
    {
        // Once the interaction period is complete we should "pick up"
        // the Pickup by parenting it to character
        if (transform.parent != localPlayerTransform)
        {
            transform.parent = localPlayerTransform;
            ConstraintSource cs = new ConstraintSource();
            cs.sourceTransform = transform;
            localPlayerTransform.gameObject.GetComponent<PositionConstraint>().AddSource(cs);
            localPlayerTransform.gameObject.GetComponent<PositionConstraint>().constraintActive = true;
        }
    }
}
