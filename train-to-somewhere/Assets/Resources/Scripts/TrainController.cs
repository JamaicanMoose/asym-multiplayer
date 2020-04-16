using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainController : MonoBehaviour
{
    // Start is called before the first frame update

    public float speed = 2f;
    public float acceleration = 0.0f;
    public float slowdownRate = .1f;

    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (speed > 0)
        {
            acceleration -= slowdownRate;
            speed += acceleration * Time.deltaTime;
        }
        else
        {
            speed = 0;
        }

        transform.position = Vector3.MoveTowards(transform.position, transform.position - new Vector3(0.0f, 0.0f, 100.0f), speed * Time.deltaTime);
    }

    public void ChangeAcceleration(float change)
    {
        acceleration += change;
    }
}
