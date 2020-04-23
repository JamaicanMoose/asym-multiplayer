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
    public float dashSpeed = 15f;
    public float dashTime = .3f;

    bool dashing = false;

   
    Rigidbody rb;
    PlayerObject player;

    UnityClient client;

    Vector3 lastPosition;
    Vector3 lastMoveVector;
    Transform mainCameraTransform;

    public Collider defaultCollider;
    public Collider holdCollider;

    public ushort parentCarID = 1;

    private void Awake()
    {
        client = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<UnityClient>();
        Assert.IsNotNull(client);

        mainCameraTransform = GameObject.FindGameObjectWithTag("MainCamera").transform;
        defaultCollider.enabled = true;
        holdCollider.enabled = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();

        player = GetComponent<PlayerObject>();

        lastPosition = transform.localPosition;
        lastMoveVector = transform.forward;
    }

    // Update is called once per frame
    void Update()
    {
     
        Vector3 moveDirection = mainCameraTransform.right * Input.GetAxis("Horizontal") + mainCameraTransform.forward * Input.GetAxis("Vertical");
      
        moveDirection.y = 0f;
        moveDirection.Normalize();
        if (Input.GetButtonDown("Jump") && moveDirection != Vector3.zero)
        {
            dashing = true;
            StartCoroutine(DashTimer());
        }
        if(dashing)
        {
        
            rb.velocity = moveDirection * dashSpeed;
        }
        else
        {
       
            rb.velocity = moveDirection * moveSpeed;
        }
        player.SetMovePosition(transform.localPosition);
      
        lastMoveVector = moveDirection;

        if (transform.forward.normalized != lastMoveVector.normalized)
        {
            transform.forward = Vector3.RotateTowards(transform.forward, lastMoveVector, 10 * Time.deltaTime, 0.0f);
            Debug.DrawRay(transform.position, lastMoveVector, Color.red);
        }
        player.SetRotation(transform.rotation.eulerAngles);

        

        //This checks if we have moved far enough from server position to send another update
        if (Vector3.Distance(lastPosition, transform.localPosition) > moveDistance)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                //The parentCarID field is updated from the TrainCarTracker script.
                writer.Write(parentCarID);

                writer.Write(transform.localPosition.x);
                writer.Write(transform.localPosition.y);
                writer.Write(transform.localPosition.z);
          

                Vector3 rotation = transform.rotation.eulerAngles;
                writer.Write(rotation.x);
                writer.Write(rotation.y);
                writer.Write(rotation.z);
                using (Message message = Message.Create(MOVEMENT_TAG, writer))
                    client.SendMessage(message, SendMode.Unreliable);
            }

            lastPosition = transform.localPosition;
        }
    }

    IEnumerator DashTimer()
    {
        yield return new WaitForSeconds(dashTime);
        dashing = false;

    }

    //Set the characters collider based on whether it is holding an object or not
    //this is currently called from the pickup script
    public void SetCollider(bool holding)
    {
        if(holding)
        {
            holdCollider.enabled = true;
            defaultCollider.enabled = false;
        }
        else
        {
            holdCollider.enabled = false;
            defaultCollider.enabled = true;
        }
    }
}
