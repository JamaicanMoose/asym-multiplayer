using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ThrowTrigger : MonoBehaviour
{
    [Serializable]
    public class ThrowEvent
    {
        public string thrownObjectTag;
        public UnityEvent onThrowReceive = new UnityEvent();
    }

    public ThrowEvent[] events;

    private void OnTriggerEnter(Collider other)
    {
        foreach (ThrowEvent te in events) {
            if (other.transform.CompareTag(te.thrownObjectTag)) {
                te.onThrowReceive.Invoke();
                Destroy(other.gameObject);
                return;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
