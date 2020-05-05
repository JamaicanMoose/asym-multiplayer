using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupVolume : MonoBehaviour
{
    public bool isServer = false;

    public List<Transform> potentialPickups = new List<Transform>();

    private void Awake()
    {
        isServer = GameObject.FindGameObjectWithTag("Network")
            .GetComponent<DarkRift.Server.Unity.XmlUnityServer>() != null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;
        if (other.CompareTag("Pickup"))
        {
            PickupGeneric pUp = other.GetComponent<PickupGeneric>();
            if(pUp.isHeld == false)
            {
                potentialPickups.Add(other.transform);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isServer) return;
        if (other.CompareTag("Pickup"))
        {
            PickupGeneric pUp = other.GetComponent<PickupGeneric>();
            if (pUp.isHeld == true)
            {
                if (potentialPickups.Contains(other.transform))
                {
                    potentialPickups.Remove(other.transform);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isServer) return;
        if (potentialPickups.Contains(other.transform))
        {
            potentialPickups.Remove(other.transform);
        }
    }
}
