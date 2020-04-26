using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TTSClient : MonoBehaviour
{
    [HideInInspector]
    public ushort localPlayerId;

    Dictionary<ushort, Transform> idMap;

    [SerializeField]
    [Tooltip("The local player prefab.")]
    public GameObject controllablePlayerPrefab;

    // Start is called before the first frame update
    void Start()
    {
        idMap = GameObject.FindGameObjectWithTag("Network").GetComponent<TTSIDMap>().idMap;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AssignLocalPlayer()
    {
        GameObject toReplace = idMap[localPlayerId].gameObject;
        ushort ttsID = toReplace.GetComponent<TTSID>().id;
        Transform parent = toReplace.transform.parent;
        Vector3 localPosition = toReplace.transform.localPosition;
        Quaternion rotation = toReplace.transform.rotation;
        Destroy(toReplace);
        GameObject localPlayer = Instantiate(controllablePlayerPrefab, localPosition, rotation, parent);
        localPlayer.GetComponent<TTSID>().id = ttsID;


    }
}
