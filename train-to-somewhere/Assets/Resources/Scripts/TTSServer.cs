using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TTSServer : MonoBehaviour
{

    //Client ID -> TTSID
    public Dictionary<ushort, ushort> clientPlayerMap;

    private void Awake()
    {
        clientPlayerMap = new Dictionary<ushort, ushort>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
