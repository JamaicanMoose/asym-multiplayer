using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempFoodBox : MonoBehaviour
{
    public void Success()
    {
        gameObject.GetComponent<Interactable>().interactingPlayerTransform.GetComponent<PlayerObject>().foodLevel += 25;
    }
}
