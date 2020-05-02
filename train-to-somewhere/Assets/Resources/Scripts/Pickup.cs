using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{

    public bool isHeld = false;
    public Transform holdingTransform = null;


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


}
