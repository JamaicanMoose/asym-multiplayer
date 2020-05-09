using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TTS
{
    public class TrainTrack : MonoBehaviour
    {
        private bool isServer = false;

        private void Awake()
        {
            isServer = GameObject.FindGameObjectWithTag("Network")
                .GetComponent<DarkRift.Server.Unity.XmlUnityServer>() != null;
        }

        private void OnTriggerExit(Collider other)
        {
            if (isServer)
            {
                if ( other.transform.name == "Train")
                {
                    Transform first = transform.parent.GetChild(0);
                    Transform last = transform.parent.GetChild(transform.parent.childCount - 1);
                    //Debug.Log($"trig: {name}; first: {first.name}; last: {last.name}");
                    first.position = last.position + -other.transform.forward * 16;
                    first.SetAsLastSibling();
                }
            }
        }
    }
}