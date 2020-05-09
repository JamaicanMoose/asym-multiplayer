using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{

    public Text timerText;
    public Text speedText;
    public Text posText;

    private int secondsLeft = 10 * 60;

    Transform train;
    float prevPos;

    bool isServer;

    private void Awake()
    {
        GameObject.FindGameObjectWithTag("Network").GetComponent<TTSGeneric>().GameStarted += StartGame;

        timerText.text = "10:00";

        isServer = GameObject.FindGameObjectWithTag("Network")
          .GetComponent<DarkRift.Server.Unity.XmlUnityServer>() != null;
    }

    void StartGame(object sender, EventArgs e)
    {
        train = GameObject.FindGameObjectWithTag("Train").transform;
        prevPos = train.position.z;
        posText.text = $"{Math.Round(Math.Abs(prevPos), 0)} m";
        InvokeRepeating("UpdateHUD", 1.0f, 1.0f);
    }

    void UpdateHUD()
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

            speedText.text = $"{Math.Round(Math.Abs(train.position.z - prevPos), 0)} m/s";
            prevPos = train.position.z;
            posText.text = $"{Math.Round(Math.Abs(prevPos), 0)} m";

            if(secondsLeft == 0 && isServer)
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
