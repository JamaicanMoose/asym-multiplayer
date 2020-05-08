using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangingCarInit : MonoBehaviour
{
    bool isServer;
    public GameObject chefRoom;
    public GameObject engineerRoom;
    public GameObject conductorRoom;

    private void Awake()
    {
        isServer = GameObject.FindGameObjectWithTag("Network")
            .GetComponent<DarkRift.Server.Unity.XmlUnityServer>() != null;

        if(isServer)
        {
            GameObject chefR = GameObject.Instantiate(chefRoom, transform.position + new Vector3(1.1f, 0, 4), Quaternion.Euler(0, -90, 0), GameObject.FindGameObjectWithTag("Train").transform);
            chefR.GetComponent<TTSID>().Init();
            GameObject engineerR = GameObject.Instantiate(engineerRoom, transform.position + new Vector3(1.1f, 0, 0), Quaternion.Euler(0, -90, 0), GameObject.FindGameObjectWithTag("Train").transform);
            engineerR.GetComponent<TTSID>().Init();
            GameObject conductorR = GameObject.Instantiate(conductorRoom, transform.position + new Vector3(1.1f, 0, -4), Quaternion.Euler(0, -90, 0), GameObject.FindGameObjectWithTag("Train").transform);
            conductorR.GetComponent<TTSID>().Init();
        }
    }
}
