using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoalBoxInteract : InteractGeneric
{
    public GameObject coalPrefab;

    public override void StartUse(Transform interactingTransform)
    {
        if (inUse)
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
        Debug.Log("Spawn coal");

        GameObject coal = Instantiate(coalPrefab, gameObject.transform.position + new Vector3(0, 1.55f, 0), Quaternion.identity);
        coal.transform.parent = GameObject.FindGameObjectWithTag("Train").transform;
        coal.GetComponent<TTSID>().Init();
        TTS.GameObjectInitMessage initMessage = new TTS.GameObjectInitMessage(coal);
        TTS.ObjectSync os = GameObject.FindGameObjectWithTag("Network").GetComponent<TTS.ObjectSync>();
        os.initBuffer.Add(initMessage);
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


}
