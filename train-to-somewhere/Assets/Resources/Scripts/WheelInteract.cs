using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelInteract : InteractGeneric
{
    public WheelBreak car;
    public bool isFront;

    public InteractVolume playerIV;

    public override void StartUse(Transform interactingTransform)
    {
        if (inUse)
        {
            Debug.LogError("Tried to use inUse object");
        }
        else
        {
            inUse = true;
            playerIV = interactingTransform.GetComponentInChildren<InteractVolume>();
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
        GetComponent<TTSID>().Remove();

        if (isFront)
        {
            car.frontBroken = false;
        } else
        {
            car.backBroken = false;
        }
        
        if (playerIV != null)
        {
            if (playerIV.potentialInteracts.Count > 0)
            {
                playerIV.potentialInteracts.RemoveAt(0);
            }
        }
    }

    IEnumerator useTimer()
    {
        yield return new WaitForSeconds(useTime);
        if (!abortedUse)
        {
            AfterUse();
            inUse = false;
        }
        abortedUse = false;
    }
}
