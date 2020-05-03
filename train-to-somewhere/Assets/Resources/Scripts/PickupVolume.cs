using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupVolume : MonoBehaviour
{
    public List<Transform> potentialPickups = new List<Transform>();

    private void OnTriggerEnter(Collider other)
    {
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
        if(potentialPickups.Contains(other.transform))
        {
            potentialPickups.Remove(other.transform);
        }
    }
}
