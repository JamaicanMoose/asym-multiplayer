using UnityEngine;
using DarkRift.Client;
using DarkRift;
using DarkRift.Client.Unity;

public class Pickup : MonoBehaviour
{
    public bool isHeld = false;
    public float pickupTime = 0f;
    public float moveDistance = 0.05f;
    public Interactable interactable;

    const ushort PICKUP_MOVE_TAG = 5;
    public ushort ID;

    public LocalPlayerController playerController;
    Transform parentCarTransform;
    NetworkObjectManager objManager;
    public Vector3 lastPostion = Vector3.zero;

    Rigidbody rb;
    Collider cd;

    private void Awake()
    {
        interactable = gameObject.GetComponent<Interactable>();
        if (interactable == null)
            interactable = gameObject.AddComponent<Interactable>() as Interactable;
        interactable.holdTime = pickupTime;
        interactable.afterUse.AddListener(OnInteractComplete);

       
        //playerController = GameObject.Find("Character").GetComponent<LocalPlayerController>();
        
        objManager = GameObject.Find("Network").GetComponent<NetworkObjectManager>();

       

       

        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        cd = GetComponent<Collider>();
        cd.enabled = true;
    }



    public void OnInteractComplete()
    {
        Debug.Log("On interact complete");
        if (isHeld)
        {     
            rb.isKinematic = false;
            cd.enabled = true;
            isHeld = false;
            playerController.SetCollider(isHeld);
        } else
        {
            transform.position = transform.parent.position + transform.parent.forward;
            transform.rotation = transform.parent.rotation;
            rb.isKinematic = true;
            cd.enabled = false;
            isHeld = true;
            playerController.SetCollider(isHeld);
        }
    }

    private void Update()
    {
        if (isHeld)
        {
            transform.position = playerController.transform.position + playerController.transform.forward;           
        }
        if(Input.GetKeyDown(KeyCode.F))
        {
            rb.isKinematic = false;
            cd.enabled = true;
            isHeld = false;
            playerController.SetCollider(isHeld);

            Vector3 forceVector = (playerController.transform.forward + Vector3.up * 2) * 3;
          
            rb.AddForce(forceVector, ForceMode.Impulse);

        }
        
    }
}
