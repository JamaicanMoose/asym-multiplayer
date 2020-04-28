using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TTSNetworkedPlayer : MonoBehaviour
{
    private Rigidbody rb;

    public float moveSpeed = 2.5f;
    public float dashSpeed = 15f;

    public Vector3 lastSyncPostion;
    private Vector3 lastMoveVector;
    private void Awake()
    {
        lastSyncPostion = transform.localPosition;
        lastMoveVector = Vector3.zero;
        rb = GetComponent<Rigidbody>();
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetVelocity(Vector3 direction, bool dashing)
    {
        Vector3 newVelocity;
        if(dashing)
        {
            newVelocity = direction * dashSpeed;
        }
        else
        {
            newVelocity = direction * moveSpeed;
        }

        rb.velocity = newVelocity;
        lastMoveVector = direction;

        if (transform.forward.normalized != lastMoveVector.normalized)
        {
            transform.forward = Vector3.RotateTowards(transform.forward, lastMoveVector, 10 * Time.deltaTime, 0.0f);
            Debug.DrawRay(transform.position, lastMoveVector, Color.red);
        }
    
    }


}
