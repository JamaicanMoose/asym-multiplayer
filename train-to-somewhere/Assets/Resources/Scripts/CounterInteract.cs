using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CounterInteract : InteractGeneric
{
    public override void StartUse(Transform interactingTransform)
    {
        if(inUse)
        {
            Debug.LogError("Tried to use inUse object");
        }
        else
        {
            inUse = true;
            StartCoroutine(useTimer());
        }
    }

    public override void AbortUse()
    {
        abortedUse = true;
        inUse = false;
    }

    public override void AfterUse()
    {
        //spawn food object here
        Debug.Log("Spawn food object");
    }

    IEnumerator useTimer()
    {
        yield return new WaitForSeconds(useTime);
        if(!abortedUse)
        {
            AfterUse();
            inUse = false;
        }
    }

    
}
