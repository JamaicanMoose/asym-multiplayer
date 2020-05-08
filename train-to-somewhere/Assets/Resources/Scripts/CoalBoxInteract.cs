using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoalBoxInteract : InteractGeneric
{
    public GameObject[] coalPrefabs;

    private Transform interactTransform = null;

    public override void StartUse(Transform interactingTransform)
    {
        if (inUse)
        {
            Debug.LogError("Tried to use inUse object");
        }
        else
        {
            inUse = true;
            interactTransform = interactingTransform;
            interactTransform.GetComponent<TTSPlayerAnimator>().SetBool(9, true);
            StartCoroutine(useTimer());
        }
    }

    public override void AbortUse()
    {
        abortedUse = true;
        inUse = false;
        interactTransform.GetComponent<TTSPlayerAnimator>().SetBool(9, false);
    }

    public override void AfterUse()
    {
        //spawn food object here
        Debug.Log("Spawn coal");
        interactTransform.GetComponent<TTSPlayerAnimator>().SetBool(9, false);
        GameObject coal = Instantiate(coalPrefabs[Random.Range(0, coalPrefabs.Length)], gameObject.transform.position + new Vector3(0, 0.5f, 0), Quaternion.identity);
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
