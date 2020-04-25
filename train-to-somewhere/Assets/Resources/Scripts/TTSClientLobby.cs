using System.Collections;
using System.Net;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;

public class TTSClientLobby : MonoBehaviour
{
    UnityClient client;
    public Text ipField;

    private void Awake()
    {
        client = gameObject.GetComponent<UnityClient>();
        client.MessageReceived += TTSMessageHandler;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TTSOnConnectClick()
    {
        IPAddress ip = IPAddress.Parse(ipField.text);
        client.Connect(ip, 4296, IPVersion.IPv4);
    }

    void TTSMessageHandler(object sender, MessageReceivedEventArgs e)
    {
        switch (e.GetMessage().Tag)
        {
            case TTSMessage.PLAYER_ASSOC:
                using (Message m = e.GetMessage() as Message)
                {
                    using (DarkRiftReader r = m.GetReader())
                    {
                        GameObject.FindGameObjectWithTag("Network").GetComponent<TTSClient>().localPlayerId = r.ReadUInt16();
                    }
                }
                break;
            case TTSMessage.GAME_OBJECT_INIT:
                using (Message m = e.GetMessage() as Message)
                {
                    using (DarkRiftReader r = m.GetReader())
                    {
                        int count = r.ReadInt32();
                        for (int i = 0; i < count; i++)
                            r.ReadSerializable<TTSGameObjectInitMessage>().Load();
                    }
                }
                break;
            case TTSMessage.START_GAME:
                //INIT STATE HERE
                GameObject.FindGameObjectWithTag("StartMenu").SetActive(false);
                break;
        }
    }
}
