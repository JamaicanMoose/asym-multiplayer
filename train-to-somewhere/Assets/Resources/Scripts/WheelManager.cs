using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelManager : MonoBehaviour
{
    public float breakChance = 0.1f;
    public float timeBeforeFirstBreak = 10.0f;
    public float breakTickRate = 1.5f;
    
    private void Start()
    {
        if (GameObject.FindGameObjectWithTag("Network").GetComponent<DarkRift.Server.Unity.XmlUnityServer>() != null)
        {
            InvokeRepeating("BreakWheel", timeBeforeFirstBreak, breakTickRate);
        }
    }

    void BreakWheel()
    {
        if (Random.value <= breakChance)
        {
            WheelBreak[] wheels = GetComponentsInChildren<WheelBreak>();

            // Randomly break either the front or back wheel in some car, unless one is already broken
            int idx = Random.Range(0, wheels.Length);
            bool isFront;
            bool frontBroken = wheels[idx].frontBroken;
            bool backBroken = wheels[idx].backBroken;

            if (frontBroken && backBroken)
            {
                return;
            }
            else if (frontBroken)
            {
                isFront = false;
            } else if (backBroken)
            {
                isFront = true;
            } else
            {
                isFront = Random.value > 0.5f;
            }

            wheels[idx].Break(isFront);
        }
    }
}
