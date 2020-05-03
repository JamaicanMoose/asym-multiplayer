using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupFood : PickupGeneric
{
    public float foodValue;


    // Update is called once per frame
    void Update()
    {
        if (isHeld)
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
        holdingTransform.GetComponent<TTSNetworkedPlayer>().EatFood(foodValue);
        GameObject.FindGameObjectWithTag("Network").GetComponent<TTS.ObjectSync>().RemoveObject(GetComponent<TTSID>().id);
        Destroy(gameObject);
    }
}
