using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CostumeSwitch : MonoBehaviour
{
    public string JobTag;

    private bool isServer;

    private void Awake()
    {
        isServer = GameObject.FindGameObjectWithTag("Network")
           .GetComponent<DarkRift.Server.Unity.XmlUnityServer>() != null;
       
    }

    private void OnTriggerEnter(Collider other)
    {
        if(isServer)
        {
            if (other.CompareTag("Player"))
            {
                other.GetComponent<TTSNetworkedPlayer>().SetJob(JobTag);
            }
        }
    
    }
}
