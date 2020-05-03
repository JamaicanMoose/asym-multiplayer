using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class TTSTrainController : MonoBehaviour
{
    bool gameStarted = false;

    List<string> trainCars = new List<string>()
        {
            "TrainCar",
            "TrainCar"
        };

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

    public void BuildTrain()
    {
        Transform previousCar = gameObject.transform.Find("TrainEngine");
        Vector3 carOffset = new Vector3(0, 0, 16);
        foreach (string carPrefabID in trainCars)
        {
            GameObject carPrefab = Resources.Load($"Prefabs/{carPrefabID}", typeof(GameObject)) as GameObject;
            GameObject car = GameObject.Instantiate(carPrefab, transform);
            car.transform.position = previousCar.position + carOffset;
            car.GetComponent<TTSID>().Init();

            previousCar.Find("Colliders/Back").gameObject.SetActive(false);

            previousCar = car.transform;
        }
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

    public void ChangeAcceleration(float change)
    {
        trainEngineAcceleration += change;
    }
}
