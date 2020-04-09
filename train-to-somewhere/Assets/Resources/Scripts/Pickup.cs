using UnityEngine;

public class Pickup : MonoBehaviour
{
    public bool isHeld = false;
    public float pickupTime = 0f;

    public Interactable interactable;

    private void Awake()
    {
        interactable = gameObject.GetComponent<Interactable>();
        if (interactable == null)
            interactable = gameObject.AddComponent<Interactable>() as Interactable;
        interactable.holdTime = pickupTime;
        interactable.afterUse.AddListener(OnInteractComplete);
    }

    public void OnInteractComplete()
    {
        if (isHeld)
        {
            transform.parent = interactable.interactingPlayerTransform.parent;
            isHeld = false;
        } else
        {
            transform.parent = interactable.interactingPlayerTransform;
            isHeld = true;
        }
    }
}
