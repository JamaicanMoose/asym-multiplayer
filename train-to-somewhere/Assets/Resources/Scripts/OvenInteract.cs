using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OvenInteract : InteractGeneric
{
    GameObject toCook;

    public override void StartUse(Transform interactingTransform)
    {
        if(inUse)
        {
            Debug.LogError("Tried to use inUse object");
        }
        else
        {
            inUse = true;
           
        }
    }

    public override void AbortUse()
    {
        throw new System.NotImplementedException();
    }

    public override void AfterUse()
    {
        throw new System.NotImplementedException();
    }

    
}
