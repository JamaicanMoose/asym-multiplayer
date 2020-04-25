using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TTSID : MonoBehaviour
{
    [HideInInspector]
    public ushort id = 0;
    
    TTSID(ushort init_id)
    {
        id = init_id;
    }

    public void Init()
    {
        if (id == 0)
            id = GameObject.FindGameObjectWithTag("Network").GetComponent<TTSIDCounter>().GenerateID();
    }
    private void Start()
    {
        Init();
    }
}
