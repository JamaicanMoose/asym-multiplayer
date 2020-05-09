using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PickupThrowable : PickupGeneric
{
 
    Rigidbody rb;

    public float throwScale = 3;

    private Collider col;

    public bool isServer;
    private void Awake()
    {
        isServer = GameObject.FindGameObjectWithTag("Network")
          .GetComponent<DarkRift.Server.Unity.XmlUnityServer>() != null;
        rb = GetComponent<Rigidbody>();
        if(!isServer)
        {
            Destroy(rb);
        }
        else
        {
            col = GetComponent<Collider>();
        }

      
    }
    // Update is called once per frame
    void Update()
    {

        if(isServer && isHeld)
        {
            transform.localPosition = holdingTransform.localPosition + holdingTransform.transform.forward;
            transform.rotation = holdingTransform.rotation;
        }
         
    }

    public override void Hold(Transform holding)
    {
        isHeld = true;
        holdingTransform = holding;

        if(isServer)
        {
            col.enabled = false;
        }
    }

    public override void Drop()
    {
        isHeld = false;
        holdingTransform = null;

        if (isServer)
        {
            rb.AddForce(Vector3.up * 2, ForceMode.Impulse);
            col.enabled = true;
        }
    }

    public override void Interact()
    {

        if (isServer)
        {
            col.enabled = true;
        }
        isHeld = false;
        Vector3 throwDirection = holdingTransform.forward + Vector3.up * 2;
        rb.velocity = Vector3.zero;
        rb.AddForce(throwDirection * throwScale, ForceMode.Impulse);
        holdingTransform = null;
        
    }


}
