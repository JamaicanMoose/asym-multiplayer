using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    public string pickupTag = "";
    public bool isHeld = false;
    public Transform holdingTransform = null;

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

    public void Hold(Transform holding)
    {
        isHeld = true;
        holdingTransform = holding;
    }

    public void Drop()
    {
        isHeld = false;
        holdingTransform = null;
    }

    public void Throw()
    {
        isHeld = false;
        Vector3 throwDirection = holdingTransform.forward + Vector3.up * 2;
        rb.velocity = Vector3.zero;
        rb.AddForce(throwDirection * throwScale, ForceMode.Impulse);
        holdingTransform = null;
        
    }


}
