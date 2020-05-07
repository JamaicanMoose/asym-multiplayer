using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TTSClientWheelController : MonoBehaviour
{

    GameObject[] WheelAnimators;

    public float wheelSpeed = 0;

    private void Awake()
    {
        GetComponent<TTSGeneric>().GameStarted += OnGameStart;
    }


    void OnGameStart(object sender, EventArgs e)
    {
        WheelAnimators = GameObject.FindGameObjectsWithTag("WheelAnimator");

        foreach (GameObject wAnim in WheelAnimators)
        {
            wAnim.GetComponent<Animator>().SetFloat("WheelSpeed", wheelSpeed);
        }
    }

    public void SetWheelSpeed(float speed)
    {
        foreach (GameObject wAnim in WheelAnimators)
        {
            wAnim.GetComponent<Animator>().SetFloat("WheelSpeed", speed);
        }
    }
}
