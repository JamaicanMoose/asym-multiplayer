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
        InvokeRepeating("BreakWheel", timeBeforeFirstBreak, breakTickRate);
    }

    void BreakWheel()
    {
        if (Random.value <= breakChance)
        {
            WheelBreak[] wheels = GetComponentsInChildren<WheelBreak>();

            // Randomly break either the front or back wheel in some car, unless one is already broken
            int idx = Random.Range(0, wheels.Length);
            bool isFront;
            
            if (wheels[idx].frontBroken)
            {
                isFront = false;
            } else if (wheels[idx].backBroken)
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
