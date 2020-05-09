using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System;

public class WheelManager : MonoBehaviour
{
    public float breakChance = 0.1f;
    public float timeBeforeFirstBreak = 10.0f;
    public float breakTickRate = 1.5f;

    TTSTrainController tc;

    private void Start()
    {
        if (GameObject.FindGameObjectWithTag("Network").GetComponent<DarkRift.Server.Unity.XmlUnityServer>() != null)
        {
            GameObject.FindGameObjectWithTag("Network").GetComponent<TTSGeneric>().GameStarted += StartGame;
            tc = GameObject.FindGameObjectWithTag("Train").GetComponent<TTSTrainController>();


        }
    }

    void StartGame(object sender, EventArgs e)
    {
        InvokeRepeating("BreakWheel", timeBeforeFirstBreak, breakTickRate);
    }

    void BreakWheel()
    {
        if(tc.speed < .1f)
        {
            breakChance = 0;
        }
        else if(tc.speed < 5f)
        {
            breakChance = .05f;
        }
        else if(tc.speed < 10f)
        {
            breakChance = .1f;
        }
        else if(tc.speed < 20f)
        {
            breakChance = .15f;
        }
        else
        {
            breakChance = .2f;
        }
        if (UnityEngine.Random.value <= breakChance)
        {
            WheelBreak[] wheels = GetComponentsInChildren<WheelBreak>();

            if (wheels.Length == 0)
            {
                return;
            }

            // Randomly break either the front or back wheel in some car, unless one is already broken
            int idx = UnityEngine.Random.Range(0, wheels.Length);
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
                isFront = UnityEngine.Random.value > 0.5f;
            }

            wheels[idx].Break(isFront);
        }
    }
}
