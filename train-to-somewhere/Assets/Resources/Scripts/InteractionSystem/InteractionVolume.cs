using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TTS
{
    public class InteractionVolume : MonoBehaviour
    {
        public List<GameObject> insideInteractionVolume = new List<GameObject>();

        void OnTriggerEnter(Collider other)
        {
            if (!insideInteractionVolume.Contains(other.gameObject) && other.gameObject.GetComponent<Interactable>())
            {
                Debug.Log($"{other.gameObject.name} Entered IV");
                insideInteractionVolume.Add(other.gameObject);
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (insideInteractionVolume.Contains(other.gameObject))
            {
                Debug.Log($"{other.gameObject.name} Exited IV");
                insideInteractionVolume.Remove(other.gameObject);
            }
        }
    }

}