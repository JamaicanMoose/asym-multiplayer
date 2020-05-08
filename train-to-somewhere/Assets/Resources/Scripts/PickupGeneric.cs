using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PickupGeneric : MonoBehaviour
{
    public bool isHeld;
    public Transform holdingTransform = null;
    public string pickupTag;

    public bool requiresCostume = false;
    public string costume;

    public abstract void Hold(Transform holding);

    public abstract void Drop();

    public abstract void Interact();

 
}
