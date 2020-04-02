using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlayerController : MonoBehaviour
{
    public float moveSpeed = 2.5f;
    CharacterController characterController;

    // Start is called before the first frame update
    void Start()
    {
        characterController = gameObject.GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
        moveDirection *= moveSpeed;
        if (Input.GetButtonDown("Jump"))
            moveDirection *= 100f;
        characterController.SimpleMove(moveDirection);
    }
}
