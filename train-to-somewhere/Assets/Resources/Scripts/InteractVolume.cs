using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractVolume : MonoBehaviour
{
    public bool isServer = false;

    public List<Transform> potentialInteracts = new List<Transform>();

    public InteractGeneric interactingObj = null;

    private void Awake()
    {
        isServer = GameObject.FindGameObjectWithTag("Network")
            .GetComponent<DarkRift.Server.Unity.XmlUnityServer>() != null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;
        if (other.CompareTag("Interact"))
        {
            InteractGeneric inter = other.GetComponent<InteractGeneric>();
            if (inter != null && inter.inUse == false)
            {
                potentialInteracts.Add(other.transform);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isServer) return;
        if (other.CompareTag("Interact"))
        {
            InteractGeneric inter = other.GetComponent<InteractGeneric>();
            if (inter != null && inter.inUse == true)
            {
                if (potentialInteracts.Contains(other.transform))
                {
                    while (potentialInteracts.Contains(other.transform))
                    {
                        potentialInteracts.Remove(other.transform);
                    }
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isServer) return;
        if(other.CompareTag("Interact"))
        {
            if (other.GetComponent<InteractGeneric>() == interactingObj)
            {
                if (interactingObj != null && interactingObj.inUse)
                {
                    interactingObj.AbortUse();
                }
            }

            if (potentialInteracts.Contains(other.transform))
            {
                while(potentialInteracts.Contains(other.transform))
                {
                    potentialInteracts.Remove(other.transform);
                }
               
            }
        }
      
    }
}
