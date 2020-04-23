using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerObject : MonoBehaviour
{

    Vector3 movePosition;
    Vector3 rotation;
    public float foodLevel = 100;

    private void Awake()
    {
        movePosition = transform.position;
        rotation = transform.rotation.eulerAngles;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = movePosition;
        transform.rotation = Quaternion.Euler(rotation.x, rotation.y, rotation.z);
    }

    internal void SetMovePosition(Vector3 newPosition)
    {
        movePosition = newPosition;
    }
    internal void SetRotation(Vector3 newRotation)
    {
        rotation = newRotation;
    }
}
