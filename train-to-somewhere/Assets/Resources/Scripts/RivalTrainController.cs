using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RivalTrainController : MonoBehaviour
{
    // Start is called before the first frame update

    public TrainController mainTrain;
    public float speed = 2f;
    public float slowdownRate = .1f;

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
        gameObject.transform.position += Vector3.forward * (speed - mainTrain.speed);
    }
}
