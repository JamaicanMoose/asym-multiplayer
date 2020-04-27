using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionTracker : MonoBehaviour
{
    public Vector3 lastSyncPostion;

    private void Awake()
    {
        lastSyncPostion = transform.localPosition;
    }

 
}
