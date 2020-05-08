using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OvenInteract : InteractGeneric
{
    GameObject toCook = null;

    Transform interactTransform = null;
    public override void StartUse(Transform interactingTransform)
    {
        if(inUse)
        {
            Debug.LogError("Tried to use inUse object");
        }
        else
        {
            if(interactingTransform.GetComponent<TTSNetworkedPlayer>().JobTag == "Chef" && toCook != null) 
            {
                interactTransform = interactingTransform;
                interactTransform.GetComponent<TTSPlayerAnimator>().SetBool(7, true);
                inUse = true;
                StartCoroutine(useTimer());
            }
            
        }
    }

    public override void AbortUse()
    {
        interactTransform.GetComponent<TTSPlayerAnimator>().SetBool(7, false);
        abortedUse = true;
        inUse = false;

    }

    public override void AfterUse()
    {
        interactTransform.GetComponent<TTSPlayerAnimator>().SetBool(7, false);
        if (interactTransform.GetComponentInChildren<PickupVolume>().potentialPickups.Contains(toCook.transform))
        {
            interactTransform.GetComponentInChildren<PickupVolume>().potentialPickups.Remove(toCook.transform);
        }

        string prefabTag = toCook.transform.GetChild(1).name;
        toCook.GetComponent<TTSID>().Remove();


        GameObject foodPrefab = Resources.Load($"Prefabs/{prefabTag}", typeof(GameObject)) as GameObject;
        GameObject cookedFood = GameObject.Instantiate(foodPrefab, transform.position + Vector3.up * 2, Quaternion.identity, GameObject.FindGameObjectWithTag("Train").transform);

        cookedFood.GetComponent<TTSID>().Init();
        TTS.GameObjectInitMessage initMessage = new TTS.GameObjectInitMessage(cookedFood);
        TTS.ObjectSync os = GameObject.FindGameObjectWithTag("Network").GetComponent<TTS.ObjectSync>();
        os.initBuffer.Add(initMessage);

        toCook = null;
        inUse = false;
    }


    IEnumerator useTimer()
    {
        yield return new WaitForSeconds(useTime);
        if (!abortedUse)
        {
            AfterUse();
            inUse = false;
        }
        abortedUse = false;
    }



    private void OnCollisionStay(Collision collision)
    {
        if(toCook == null && collision.gameObject.CompareTag("Pickup"))
        {
            if(collision.gameObject.GetComponent<PickupThrowable>() != null && collision.gameObject.GetComponent<PickupThrowable>().isHeld == false && collision.transform.GetChild(0).CompareTag("Ice"))
            {
                toCook = collision.gameObject;

                toCook.GetComponent<PickupThrowable>().isHeld = true;
                toCook.GetComponent<PickupThrowable>().isServer = false;
                toCook.transform.position = transform.position + Vector3.up * 2;
                toCook.transform.rotation = Quaternion.identity;
                toCook.GetComponent<Rigidbody>().isKinematic = true;
            }
        }
    }
}
