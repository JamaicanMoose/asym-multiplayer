using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DarkRift;

namespace TTS
{
    public class Interactable : MonoBehaviour
    {
        public bool inUse = false;
        public float holdTime = 2.0f;
        public Transform interactingPlayerTransform;
        public UnityEvent startUse = new UnityEvent();
        public UnityEvent duringUse = new UnityEvent();
        public UnityEvent afterUse = new UnityEvent();
        public UnityEvent abortUse = new UnityEvent();

        public void StartUse(Transform interactingPlayerTransform)
        {
            inUse = true;
            this.interactingPlayerTransform = interactingPlayerTransform;
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
}
