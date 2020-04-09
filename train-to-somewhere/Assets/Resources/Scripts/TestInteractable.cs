using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestInteractable : MonoBehaviour
{

    public void Success()
    {
        gameObject.GetComponent<Renderer>().material.color = Color.green;
    }

    public void Started()
    {
        gameObject.GetComponent<Renderer>().material.color = Color.cyan;
    }

    public void Aborted()
    {
        gameObject.GetComponent<Renderer>().material.color = Color.red;
    }
}
