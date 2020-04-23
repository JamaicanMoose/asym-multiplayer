using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerObject : MonoBehaviour
{

    Vector3 movePosition;
    Vector3 rotation;

    private void Awake()
    {
        movePosition = transform.localPosition;
        Debug.Log("Move position is: " + movePosition);
        rotation = transform.rotation.eulerAngles;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.localPosition = movePosition;
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
