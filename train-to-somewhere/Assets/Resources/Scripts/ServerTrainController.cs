using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using DarkRift;
using DarkRift.Server;
using DarkRift.Server.Unity;

public class ServerTrainController : MonoBehaviour
{
    public float speed = 0f;
    public float acceleration = 0.0f;
    public float slowdownRate = 0.0f;

    public float moveDistance = .05f;

    private Vector3 lastPosition;

    XmlUnityServer darkRiftServer;

    ushort trainID;
    private void Awake()
    {
        GameObject network = GameObject.FindGameObjectWithTag("Network");
        network.GetComponent<TTSGeneric>().GameStarted += StartTrain;
    }

    // Start is called before the first frame update
    void Start()
    {
        darkRiftServer = GameObject.FindGameObjectWithTag("Network").GetComponent<XmlUnityServer>();
        lastPosition = transform.localPosition;
        trainID = gameObject.GetComponent<TTSID>().id;
    }

    private void StartTrain(object sender, EventArgs e)
    {
        speed = 2f;
    }

    // Update is called once per frame
    void Update()
    {
        if (speed > 0)
        {
            acceleration -= slowdownRate;
            speed += acceleration * Time.deltaTime;
        }
        else
        {
            speed = 0;
        }

        transform.localPosition = Vector3.MoveTowards(transform.localPosition, transform.localPosition - new Vector3(0.0f, 0.0f, 100.0f), speed * Time.deltaTime);

        if (Vector3.Distance(transform.localPosition, lastPosition) > moveDistance)
        {
            using (DarkRiftWriter trainPositionWriter = DarkRiftWriter.Create())
            {

                trainPositionWriter.Write(new TTSGameObjectSyncMessage(trainID, transform));

                using (Message playerPositionMessage = Message.Create(TTSMessage.GAME_OBJECT_SYNC, trainPositionWriter))
                {
                    foreach (IClient c in darkRiftServer.Server.ClientManager.GetAllClients())
                        c.SendMessage(playerPositionMessage, SendMode.Unreliable);
                }
            }
            lastPosition = transform.localPosition;
        }
    }

    public void ChangeAcceleration(float change)
    {
        acceleration += change;
    }
}
