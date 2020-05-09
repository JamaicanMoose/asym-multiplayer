using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupFood : PickupGeneric
{
    public float foodValue;
    Rigidbody rb;
    bool isServer;

    private void Awake()
    {
        isServer = GameObject.FindGameObjectWithTag("Network")
          .GetComponent<DarkRift.Server.Unity.XmlUnityServer>() != null;
        rb = GetComponent<Rigidbody>();
        if (!isServer)
        {
            Destroy(rb);
        }


    }

    // Update is called once per frame
    void Update()
    {
        if (isServer && isHeld)
        {
            transform.localPosition = holdingTransform.localPosition + holdingTransform.transform.forward;
            transform.rotation = holdingTransform.rotation;
        }

    }

    public override void Hold(Transform holding)
    {
        isHeld = true;
        holdingTransform = holding;
    }

    public override void Drop()
    {
        isHeld = false;
        holdingTransform = null;
    }

    public override void Interact()
    {
        holdingTransform.GetComponent<TTSNetworkedPlayer>().EatFood(foodValue, GetComponent<TTSID>());
        
    }

}
