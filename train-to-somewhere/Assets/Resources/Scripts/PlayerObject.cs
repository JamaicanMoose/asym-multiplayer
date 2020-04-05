using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerObject : MonoBehaviour
{

    Vector3 movePosition;

    private void Awake()
    {
        movePosition = transform.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = movePosition;
    }

    internal void SetMovePosition(Vector3 newPosition)
    {
        movePosition = newPosition;
    }
}
