using System.Collections;
using System.Collections.Generic;
using DarkRift;
using DarkRift.Client.Unity;
using DarkRift.Client;
using UnityEngine;
using UnityEngine.Assertions;

public class LocalPlayerController : MonoBehaviour
{
   
    public float dashFoodCost = .1f;

    [SerializeField]
    [Tooltip("The distance we can move before we send a position update.")]
    float moveDistance = 0.05f;

    public float moveSpeed = 2.5f;
    public float dashSpeed = 15f;
    public float dashTime = .3f;

    bool dashing = false;
    bool lastDashing = false;
   
    Rigidbody rb;


    Vector3 lastPosition;
    Vector3 lastMoveVector;
    Transform mainCameraTransform;

    public Collider defaultCollider;
    public Collider holdCollider;

    private UnityClient client;
    private void Awake()
    {
        client = GameObject.Find("Network").GetComponent<UnityClient>();

        mainCameraTransform = GameObject.FindGameObjectWithTag("MainCamera").transform;
        defaultCollider.enabled = true;
        holdCollider.enabled = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();



        lastPosition = transform.localPosition;
        lastMoveVector = Vector3.zero; ;
    }

    // Update is called once per frame
    void Update()
    {
     
        Vector3 moveDirection = mainCameraTransform.right * Input.GetAxis("Horizontal") + mainCameraTransform.forward * Input.GetAxis("Vertical");
      
        moveDirection.y = 0f;
        moveDirection.Normalize();

        dashing = Input.GetKey(KeyCode.Space);

        if(moveDirection != lastMoveVector || dashing != lastDashing)
        {
            using (DarkRiftWriter moveInputWriter = DarkRiftWriter.Create())
            {

                moveInputWriter.Write(new TTSInputMessage(moveDirection, dashing));
                using (Message moveInputMessage = Message.Create(TTSMessage.MOVEMENT_INPUT, moveInputWriter))
                {
                    client.SendMessage(moveInputMessage, SendMode.Unreliable);
                }
            }

        }    

        lastMoveVector = moveDirection;
        lastDashing = dashing;
        /*
        if (Input.GetButtonDown("Jump") && moveDirection != Vector3.zero)
        {
            float costPerDash = dashFoodCost * (dashTime / Time.deltaTime);
            Debug.Log(costPerDash);
            if (player.foodLevel >= costPerDash)
            {
                dashing = true;
                StartCoroutine(DashTimer());
            }
            else
            {
                // Do some animation to indicate that the player is exhausted
            }
        }
        if(dashing)
        {
            player.foodLevel -= dashFoodCost;
            rb.velocity = moveDirection * dashSpeed;
        }
        else
        {
       
            rb.velocity = moveDirection * moveSpeed;
        }
       player.SetMovePosition(transform.localPosition);
      
       

        if (transform.forward.normalized != lastMoveVector.normalized)
        {
            transform.forward = Vector3.RotateTowards(transform.forward, lastMoveVector, 10 * Time.deltaTime, 0.0f);
            Debug.DrawRay(transform.position, lastMoveVector, Color.red);
        }
        player.SetRotation(transform.rotation.eulerAngles);
        */

    }
    /*
    IEnumerator DashTimer()
    {
        yield return new WaitForSeconds(dashTime);
        dashing = false;
    }
    */
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
