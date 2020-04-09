using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public bool inUse = false;
    public float holdTime = 2.0f;
    public UnityEvent startUse = new UnityEvent();
    public UnityEvent duringUse = new UnityEvent();
    public UnityEvent afterUse = new UnityEvent();
    public UnityEvent abortUse = new UnityEvent();

    public void StartUse()
    {
        if (startUse != null)
        {
            startUse.Invoke();
        }
    }

    public void DuringUse()
    {
        if (duringUse != null)
        {
            duringUse.Invoke();
        }
    }

    public void AfterUse()
    {
        if (afterUse != null)
        {
            afterUse.Invoke();
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
