using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class HUDTimer : MonoBehaviour
{

    public Text timerText;

    private int secondsLeft = 10 * 60;

    private void Awake()
    {
        GameObject.FindGameObjectWithTag("Network").GetComponent<TTSGeneric>().GameStarted += StartTimer;

        timerText.text = "10:00";
    }

    void StartTimer(object sender, EventArgs e)
    {
        InvokeRepeating("UpdateTimer", 1.0f, 1.0f);
    }

    void UpdateTimer()
    {
        secondsLeft -= 1;

        int minutes = secondsLeft / 60;

        int seconds = secondsLeft % 60;

        if(seconds < 10)
        {
            timerText.text = minutes + ":0" + seconds;
        }
        else
        {
            timerText.text = minutes + ":" + seconds;
        }
        
    }
}
