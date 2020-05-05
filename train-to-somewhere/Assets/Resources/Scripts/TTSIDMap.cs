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

    public void setTransform(Transform t)
    {
        idMap[t.gameObject.GetComponent<TTSID>().id] = t;
    }

    public void addTransform(ushort id, Transform t)
    {
        idMap.Add(id, t);
    }

    public void removeTransform(ushort id)
    {
        idMap.Remove(id);
    }

    public Transform getTransform(ushort id)
    {
        if (idMap.ContainsKey(id))
            return idMap[id];
        else
            return null;
    }
}
