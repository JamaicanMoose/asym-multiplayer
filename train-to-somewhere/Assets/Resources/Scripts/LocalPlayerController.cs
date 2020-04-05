using System.Collections;
using System.Collections.Generic;
using DarkRift;
using DarkRift.Client.Unity;
using DarkRift.Client;
using UnityEngine;

public class LocalPlayerController : MonoBehaviour
{
    ushort MOVEMENT_TAG = 1;

    [SerializeField]
    [Tooltip("The distance we can move before we send a position update.")]
    float moveDistance = 0.05f;

    public float moveSpeed = 2.5f;
    CharacterController characterController;
    PlayerObject player;

    public UnityClient Client;

    Vector3 lastPosition;

    // Start is called before the first frame update
    void Start()
    {
        characterController = gameObject.GetComponent<CharacterController>();
        player = GetComponent<PlayerObject>();

        lastPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
        moveDirection *= moveSpeed;
        if (Input.GetButtonDown("Jump"))
            moveDirection *= 100f;
        characterController.SimpleMove(moveDirection);
        player.SetMovePosition(transform.position);

        if (Vector3.Distance(lastPosition, transform.position) > moveDistance)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(transform.position.x);
                writer.Write(transform.position.z);

                using (Message message = Message.Create(MOVEMENT_TAG, writer))
                    Client.SendMessage(message, SendMode.Unreliable);
            }

            lastPosition = transform.position;
        }

    }
}
