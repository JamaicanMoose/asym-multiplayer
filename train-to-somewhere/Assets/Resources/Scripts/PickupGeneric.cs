using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PickupGeneric : MonoBehaviour
{
    public bool isHeld;
    public string pickupTag;

    public abstract void Hold(Transform holding);

    public abstract void Drop();

    public abstract void Interact();

 
}
