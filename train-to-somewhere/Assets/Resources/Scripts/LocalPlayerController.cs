using System.Collections;
using System.Collections.Generic;
using DarkRift;
using DarkRift.Client.Unity;
using DarkRift.Client;
using UnityEngine;
using UnityEngine.Assertions;

public class LocalPlayerController : MonoBehaviour
{
    ushort MOVEMENT_TAG = 1;

    [SerializeField]
    [Tooltip("The distance we can move before we send a position update.")]
    float moveDistance = 0.05f;

    public float moveSpeed = 2.5f;
    CharacterController characterController;
    PlayerObject player;

    UnityClient client;

    Vector3 lastPosition;
    Vector3 lastMoveVector;
    Transform mainCameraTransform;

    private void Awake()
    {
        client = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<UnityClient>();
        Assert.IsNotNull(client);

        mainCameraTransform = GameObject.FindGameObjectWithTag("MainCamera").transform;
    }

    // Start is called before the first frame update
    void Start()
    {
        characterController = gameObject.GetComponent<CharacterController>();
        player = GetComponent<PlayerObject>();

        lastPosition = transform.position;
        lastMoveVector = transform.forward;
    }

    // Update is called once per frame
    void Update()
    {
        //Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
        Vector3 moveDirection = mainCameraTransform.right * Input.GetAxis("Horizontal") + mainCameraTransform.forward * Input.GetAxis("Vertical");
        moveDirection.y = 0f;
        if (Input.GetButtonDown("Jump") && moveDirection != Vector3.zero)
        {
            characterController.SimpleMove(moveDirection * moveSpeed * 100f);
            transform.forward = moveDirection;
        } else
        {
            characterController.SimpleMove(moveDirection * moveSpeed);
        }
        player.SetMovePosition(transform.position);
        lastMoveVector = moveDirection;

        if (transform.forward.normalized != lastMoveVector.normalized)
        {
            transform.forward = Vector3.RotateTowards(transform.forward, lastMoveVector, 10 * Time.deltaTime, 0.0f);
            Debug.DrawRay(transform.position, lastMoveVector, Color.red);
        }

        if (Vector3.Distance(lastPosition, transform.position) > moveDistance)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(transform.position.x);
                writer.Write(transform.position.z);

                using (Message message = Message.Create(MOVEMENT_TAG, writer))
                    client.SendMessage(message, SendMode.Unreliable);
            }

            lastPosition = transform.position;
        }
    }
}
