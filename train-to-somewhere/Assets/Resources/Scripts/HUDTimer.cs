using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class HUDTimer : MonoBehaviour
{

    public Text timerText;

    private int secondsLeft = 60 * 10;

    bool isServer;

    private void Awake()
    {
        GameObject.FindGameObjectWithTag("Network").GetComponent<TTSGeneric>().GameStarted += StartTimer;

        timerText.text = "10:00";

        isServer = GameObject.FindGameObjectWithTag("Network")
            .GetComponent<DarkRift.Server.Unity.XmlUnityServer>() != null;
    }

    void StartTimer(object sender, EventArgs e)
    {
        InvokeRepeating("UpdateTimer", 1.0f, 1.0f);
    }

    void UpdateTimer()
    {
        if(secondsLeft > 0)
        {
            secondsLeft -= 1;

            int minutes = secondsLeft / 60;

            int seconds = secondsLeft % 60;

            if (seconds < 10)
            {
                timerText.text = minutes + ":0" + seconds;
            }
            else
            {
                timerText.text = minutes + ":" + seconds;
            }

            if (secondsLeft == 0 && isServer)
            {
                SetWin();
            }
        }

    }

    void SetWin()
    {
        foreach(Transform t in GameObject.FindGameObjectWithTag("Network").GetComponent<TTSIDMap>().idMap.Values)
        {
            if(t.CompareTag("Player"))
            {
                t.GetComponent<TTSPlayerAnimator>().SetTrigger(14);
            }
          
        }
    }


}
