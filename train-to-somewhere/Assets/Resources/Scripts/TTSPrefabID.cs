using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TTSPrefabID : MonoBehaviour
{
    public string prefabID;

    public static string Get(GameObject go)
    {
        TTSPrefabID p = go.GetComponent<TTSPrefabID>();
        return (p == null ? "" : p.prefabID);
    }

    public static string Get(Transform t) { return Get(t.gameObject); }
}
