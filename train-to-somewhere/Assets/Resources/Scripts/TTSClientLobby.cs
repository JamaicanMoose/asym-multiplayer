using System.Collections;
using System.Net;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;

public class TTSClientLobby : MonoBehaviour
{
    UnityClient client;
    public Text ipField;
    TTSClient ttsClient;

    public GameObject connectButton;

    private bool connected = false;

    private void Awake()
    {
        client = gameObject.GetComponent<UnityClient>();
        client.MessageReceived += TTSMessageHandler;
        client.Disconnected += TTSDisconnectHandler;

        ttsClient = gameObject.GetComponent<TTSClient>();
    }

    public void TTSOnConnectClick()
    {
        if(!connected)
        {
            IPAddress ip = IPAddress.Parse(ipField.text);
            client.Connect(ip, 4296, IPVersion.IPv4);
        }

        if (client.ConnectionState == ConnectionState.Connected)
        {
            connectButton.GetComponent<Image>().color = Color.green;
            connectButton.GetComponentInChildren<Text>().text = "Waiting for Host...";
            connected = true;
        }          
        else
        {
            connected = false;
        }
    }

    void TTSDisconnectHandler(object sender, DisconnectedEventArgs e)
    {
        SceneManager.LoadScene(0);
    }

    void TTSMessageHandler(object sender, MessageReceivedEventArgs e)
    {
        switch ((TTS.MessageType)e.GetMessage().Tag)
        {
            case TTS.MessageType.PLAYER_ASSOC:
                using (Message m = e.GetMessage() as Message)
                    using (DarkRiftReader r = m.GetReader())
                        GameObject.FindGameObjectWithTag("Network")
                            .GetComponent<TTSClient>().localPlayerId = r.ReadUInt16();
                break;
            case TTS.MessageType.START_GAME:
                StartCoroutine(StartGameHandler());
                break;
        }
    }

    IEnumerator StartGameHandler()
    {
        TTSClient client = GameObject.FindGameObjectWithTag("Network").GetComponent<TTSClient>();
        while (!client.initSyncFinished || client.localPlayerId == 0) yield return new WaitForSeconds(.1f);
        client.GameStart();
        GameObject.FindGameObjectWithTag("StartMenu").SetActive(false);
    }
}
