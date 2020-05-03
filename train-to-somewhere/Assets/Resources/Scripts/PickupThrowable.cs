﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class PickupThrowable : PickupGeneric
{
 
    Rigidbody rb;

    public float throwScale = 3;



    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    // Update is called once per frame
    void Update()
    {
        if(isHeld)
        {
            transform.localPosition = holdingTransform.localPosition + holdingTransform.transform.forward;
            transform.rotation = holdingTransform.rotation;
        }
         
    }

    public override void Hold(Transform holding)
    {
        isHeld = true;
        holdingTransform = holding;
    }

    public override void Drop()
    {
        isHeld = false;
        holdingTransform = null;
    }

    public override void Interact()
    {
        isHeld = false;
        Vector3 throwDirection = holdingTransform.forward + Vector3.up * 2;
        rb.velocity = Vector3.zero;
        rb.AddForce(throwDirection * throwScale, ForceMode.Impulse);
        holdingTransform = null;
        
    }


}
