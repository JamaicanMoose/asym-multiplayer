using UnityEngine;
using DarkRift.Client.Unity;
using DarkRift;
using UnityEngine.Assertions;

public class PlayerInteract : MonoBehaviour
{
    public float interactDistance = 1.0f;
    public float groundY = 2.94f;

    private Interactable inter = null;

    private float time;

    InteractionVolume interactionVolume;

    private UnityClient Client;
    private NetworkObjectManager objManager;
    const ushort USE_TAG = 6;

    const ushort START_USE = 1;
    const ushort DURING_USE = 2;
    const ushort AFTER_USE = 3;
    const ushort ABORT_USE = 4;

    private void Awake()
    {
        interactionVolume = gameObject.transform.Find("Interaction Volume").GetComponent<InteractionVolume>();
        Client = GameObject.Find("Network").GetComponent<UnityClient>();
        Assert.IsNotNull(interactionVolume);
        objManager = GameObject.Find("Network").GetComponent<NetworkObjectManager>();
    }

    // Update is called once per frame
    void Update()
    {
        // If we are currently holding an interactable, 
        // we should always be interacting with that interactable.
        Pickup holding = gameObject.GetComponentInChildren<Pickup>();
        if (holding != null)
        {
            inter = holding.interactable;
            inter.interactingPlayerTransform = transform;
        }

        // If the interactable we are currently interacting with is no longer in the volume then abort.
        else if (inter != null && !interactionVolume.insideInteractionVolume.Exists(g => g.GetComponent<Interactable>() == inter))
        {
            inter.AbortUse();
            if (objManager.objects[inter.gameObject.GetComponent<NetworkTrackable>().uniqueID].ObjType != 1)
                SendUseMessage(inter.gameObject.GetComponent<NetworkTrackable>().uniqueID, ABORT_USE);
            inter = null;
        }

        foreach (GameObject g in interactionVolume.insideInteractionVolume)
        {
            if (inter != null)
                break;
            if (g.GetComponent<Interactable>().interactingPlayerTransform != null)
                continue;
            inter = g.GetComponent<Interactable>();
        }


        //if the interactable we're currently interacting with leaves the interaction volume we should abort

        if (inter)
        {
            // Code from https://forum.unity.com/threads/solved-hold-button-for-3-seconds.451812/
            // Start the hold timer
            if (Input.GetButtonDown("Fire1") && !inter.inUse)
            {
                time = Time.time;
                inter.interactingPlayerTransform = transform;
                inter.StartUse();

                //1 is tag for pickups
                if(objManager.objects[inter.gameObject.GetComponent<NetworkTrackable>().uniqueID].ObjType != 1)
                {
                    SendUseMessage(inter.gameObject.GetComponent<NetworkTrackable>().uniqueID, START_USE);
                }
                     
          
            }
            else if (Input.GetButton("Fire1"))
            {
                if (Time.time - time > inter.holdTime)
                {
                    time = float.PositiveInfinity;
                    inter.AfterUse();
                    if (objManager.objects[inter.gameObject.GetComponent<NetworkTrackable>().uniqueID].ObjType != 1)
                        SendUseMessage(inter.gameObject.GetComponent<NetworkTrackable>().uniqueID, AFTER_USE);
                    inter = null;
                }
                else
                {
                    if (inter.inUse)
                    {
                        inter.DuringUse();
                        if (objManager.objects[inter.gameObject.GetComponent<NetworkTrackable>().uniqueID].ObjType != 1)
                            SendUseMessage(inter.gameObject.GetComponent<NetworkTrackable>().uniqueID, DURING_USE);
                    }
                }
                // Cancel the hold timer if let go
            }
            else
            {
                time = float.PositiveInfinity;
                if (inter.inUse)
                {
                    inter.AbortUse();
                    if (objManager.objects[inter.gameObject.GetComponent<NetworkTrackable>().uniqueID].ObjType != 1)
                        SendUseMessage(inter.gameObject.GetComponent<NetworkTrackable>().uniqueID, ABORT_USE);
                }
                inter = null;
            }
        }
    }

    //This function sends the use state of an object to the server
    void SendUseMessage(ushort objID, ushort useTag)
    {
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(objID);
            writer.Write(useTag);
            using (Message useMessage = Message.Create(USE_TAG, writer))
            {
                Client.SendMessage(useMessage, SendMode.Reliable);
            }
        }

    }
}
