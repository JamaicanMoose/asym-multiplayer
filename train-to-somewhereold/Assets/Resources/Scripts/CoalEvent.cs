using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoalEvent : MonoBehaviour
{
    public float accelerationChange = 0.5f;

    private TrainController tc;

    private void Start()
    {
        tc = GameObject.FindGameObjectWithTag("Train").GetComponent<TrainController>();
    }

    public void SpeedUp()
    {
        tc.ChangeAcceleration(accelerationChange);
    }
}
