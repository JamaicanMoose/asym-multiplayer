using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractGeneric : MonoBehaviour
{
    public bool inUse = false;
    public bool abortedUse = false;
    public float useTime;

    public bool requiresCostume = false;
    public string costume;

    public abstract void StartUse(Transform interactingTransform);

    public abstract void AbortUse();

    public abstract void AfterUse();
}
