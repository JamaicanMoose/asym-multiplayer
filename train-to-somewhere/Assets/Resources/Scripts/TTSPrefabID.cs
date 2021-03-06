﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TTSPrefabID : MonoBehaviour
{
    public string prefabID;
    private bool isServer;

    private void Awake()
    {
        isServer = GameObject.FindGameObjectWithTag("Network")
            .GetComponent<DarkRift.Server.Unity.XmlUnityServer>() != null;
    }

    public static string Get(GameObject go)
    {
        TTSPrefabID p = go.GetComponent<TTSPrefabID>();
        return (p == null ? "" : p.prefabID);
    }

    public static string Get(Transform t) { return Get(t.gameObject); }
}
