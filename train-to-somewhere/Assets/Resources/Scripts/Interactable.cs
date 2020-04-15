using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public bool inUse = false;
    public float holdTime = 2.0f;
    public Transform interactingPlayerTransform;
    public UnityEvent startUse = new UnityEvent();
    public UnityEvent duringUse = new UnityEvent();
    public UnityEvent afterUse = new UnityEvent();
    public UnityEvent abortUse = new UnityEvent();

    public void Awake()
    {
        if (gameObject.GetComponent<NetworkTrackable>() == null)
        {
            NetworkTrackable trackable = gameObject.AddComponent<NetworkTrackable>() as NetworkTrackable;
            //trackable.uniqueID = System.Guid.NewGuid().ToString();
        }
    }

    public void StartUse()
    {
        inUse = true;
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
        inUse = false;
        if (afterUse != null)
        {
            afterUse.Invoke();
        }
        interactingPlayerTransform = null;
    }

    public void AbortUse()
    {
        inUse = false;
        if (abortUse != null)
        {
            abortUse.Invoke();
        }
        interactingPlayerTransform = null;
    }
}
