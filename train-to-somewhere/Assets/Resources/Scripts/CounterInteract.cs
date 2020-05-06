using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CounterInteract : InteractGeneric
{
    public GameObject[] foods;

    public override void StartUse(Transform interactingTransform)
    {
        if(inUse)
        {
            Debug.LogError("Tried to use inUse object");
        }
        else
        {
            inUse = true;
            StartCoroutine(useTimer());
        }
    }

    public override void AbortUse()
    {
        abortedUse = true;
        inUse = false;
    }

    public override void AfterUse()
    {
        //spawn food object here
        Debug.Log("Spawn food object");
        
        GameObject food = Instantiate(foods[Random.Range(0, foods.Length)], gameObject.transform.position + new Vector3(0, 1.55f, 0), Quaternion.identity);
        food.transform.parent = GameObject.FindGameObjectWithTag("Train").transform;
        food.GetComponent<TTSID>().Init();
        TTS.GameObjectInitMessage initMessage = new TTS.GameObjectInitMessage(food);
        TTS.ObjectSync os = GameObject.FindGameObjectWithTag("Network").GetComponent<TTS.ObjectSync>();
        os.initBuffer.Add(initMessage);
        inUse = false;
    }

    IEnumerator useTimer()
    {
        yield return new WaitForSeconds(useTime);
        if(!abortedUse)
        {
            AfterUse();
            inUse = false;
        }
    }

    
}
