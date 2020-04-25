using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TTSIDMap : MonoBehaviour
{

    [HideInInspector]
    public Dictionary<ushort, Transform> idMap;

    private void Awake()
    {
        idMap = new Dictionary<ushort, Transform>();
        idMap[0] = GameObject.Find("World").transform;
    }
}
