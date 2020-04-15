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
    ushort ID;
    private UnityClient client;

    Vector3 lastPostion;

    private void Awake()
    {
        interactable = gameObject.GetComponent<Interactable>();
        if (interactable == null)
            interactable = gameObject.AddComponent<Interactable>() as Interactable;
        interactable.holdTime = pickupTime;
        interactable.afterUse.AddListener(OnInteractComplete);

        client = GameObject.Find("Network").GetComponent<UnityClient>();
        lastPostion = transform.position;

        ID = GetComponent<NetworkTrackable>().uniqueID;
    }

    public void OnInteractComplete()
    {
        if (isHeld)
        {
            transform.parent = interactable.interactingPlayerTransform.parent;
            isHeld = false;
        } else
        {
            transform.parent = interactable.interactingPlayerTransform;
            isHeld = true;
        }
    }

    private void Update()
    {
        if(isHeld)
        {
            if (Vector3.Distance(transform.position, lastPostion) > moveDistance)
            {
                lastPostion = transform.position;
              
                using (DarkRiftWriter writer = DarkRiftWriter.Create())
                {
                    writer.Write(GetComponent<NetworkTrackable>().uniqueID);
                    if (isHeld)
                    {
                        if (transform.parent.parent.GetComponent<NetworkTrackable>() != null)
                            writer.Write(transform.parent.parent.GetComponent<NetworkTrackable>().uniqueID);
                        else
                        {
                            ushort defaultID = 0;
                            writer.Write(defaultID);
                        }
                        Vector3 positionRelativetoCar = transform.parent.parent.InverseTransformPoint(transform.position);
                        writer.Write(positionRelativetoCar.x);
                        writer.Write(positionRelativetoCar.y);
                        writer.Write(positionRelativetoCar.z);
                    }
                    else
                    {
                        if (transform.parent != null)
                        {

                            writer.Write(transform.parent.GetComponent<NetworkTrackable>().uniqueID);
                            writer.Write(transform.localPosition.x);
                            writer.Write(transform.localPosition.y);
                            writer.Write(transform.localPosition.z);
                        }
                        else
                        {
                            ushort defaultID = 0;
                            writer.Write(defaultID);
                            Vector3 defaultPosition = Vector3.zero;
                            writer.Write(defaultPosition.x);
                            writer.Write(defaultPosition.y);
                            writer.Write(defaultPosition.z);
                        }


                    }

                    Vector3 angles = transform.rotation.eulerAngles;
                    writer.Write(angles.x);
                    writer.Write(angles.y);
                    writer.Write(angles.z);

                    using (Message message = Message.Create(PICKUP_MOVE_TAG, writer))
                        client.SendMessage(message, SendMode.Unreliable);
                }
            }
        }
        
    }
}
