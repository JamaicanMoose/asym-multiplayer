using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public bool inUse = false;
    public float holdTime = 2.0f;
    public UnityEvent afterUse;
    public UnityEvent duringUse;
    public UnityEvent abortUse;

    // Call this when the object has been fully interacted with
    public void AfterUse()
    {
        if (afterUse != null)
        {
            afterUse.Invoke();
        }
    }

    public void DuringUse()
    {
        if (duringUse != null)
        {
            duringUse.Invoke();
        }
    }

    public void AbortUse()
    {
        if (abortUse != null)
        {
            abortUse.Invoke();
        }
    }
}
