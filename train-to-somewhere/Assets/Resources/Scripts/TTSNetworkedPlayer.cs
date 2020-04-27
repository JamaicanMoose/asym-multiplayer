using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TTSNetworkedPlayer : MonoBehaviour
{
    private Rigidbody rb;

    public float moveSpeed = 2.5f;
    public float dashSpeed = 15f;

    private void Awake()
    {
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
    }


}
