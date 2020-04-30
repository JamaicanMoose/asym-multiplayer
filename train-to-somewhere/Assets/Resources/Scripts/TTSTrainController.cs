using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class TTSTrainController : MonoBehaviour
{
    bool gameStarted = false;

    public float speed = 2f;
    public float trainEngineAcceleration = 0.0f;
    public float maxTrainEngineAcceleration = 7.0f;
    [Tooltip("Rate at which trainEngineAcceleration decreases.")]
    public float trainEngineCooldownRate = 0.2f;
    public float frictionAcceleration = 0.1f;

    private void Awake()
    {
        GameObject.FindGameObjectWithTag("Network").GetComponent<TTSGeneric>().GameStarted += StartTrain;
    }

    private void StartTrain(object sender, EventArgs e)
    {
        gameStarted = true;
    }

    private void FixedUpdate()
    {
        if (gameStarted)
        {
            // Every physics timestep the engine's output is decreased at the cooldown rate
            if (trainEngineAcceleration > trainEngineCooldownRate)
                trainEngineAcceleration = Math.Sign(trainEngineAcceleration) * (Math.Abs(trainEngineAcceleration) - (trainEngineCooldownRate * Time.fixedDeltaTime));
            else
                trainEngineAcceleration = 0;

            // Every physics timestep the train moves according to the train engine's output
            float accel = 0f;
            if (trainEngineAcceleration > frictionAcceleration)
                accel = Math.Sign(trainEngineAcceleration) * (Math.Abs(trainEngineAcceleration) - frictionAcceleration);
            speed += accel * Time.fixedDeltaTime;
            Vector3 forward = -transform.forward;
            transform.Translate(forward * speed * Time.fixedDeltaTime);
        }
    }
}
