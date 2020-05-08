using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ThrowReceiver : MonoBehaviour
{
    [Serializable]
    public class ThrowEvent
    {
        public string thrownObjectPickupTag;
        public UnityEvent onThrowReceive = new UnityEvent();
    }

    public ThrowEvent[] events;

    private void OnTriggerEnter(Collider other)
    {
        foreach (ThrowEvent te in events)
        {
            PickupGeneric p = other.GetComponent<PickupGeneric>();
            if (p && p.pickupTag == te.thrownObjectPickupTag)
            {
                te.onThrowReceive.Invoke();
                other.GetComponent<TTSID>().Remove();
                return;
            }
        }
    }

}