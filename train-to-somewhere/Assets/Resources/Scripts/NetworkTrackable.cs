using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

// A game object that should be identifiable across clients.
public class NetworkTrackable : MonoBehaviour
{
    public string uniqueID;

    private void Awake()
    {
        Assert.IsNotNull(uniqueID, $"{gameObject.name}: NetworkTrackables must have a unique ID.");
    }
}
