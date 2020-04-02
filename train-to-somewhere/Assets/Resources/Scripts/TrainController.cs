using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainController : MonoBehaviour
{
    // Start is called before the first frame update

    public float speed = 2f;
    public float slowdownRate = .1f;
    public ScrollingBackground backgroundController;

    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        if (speed > 0)
            speed -= slowdownRate * Time.deltaTime;
        else
            speed = 0;
        backgroundController.bgSpeed = speed;
    }
}
