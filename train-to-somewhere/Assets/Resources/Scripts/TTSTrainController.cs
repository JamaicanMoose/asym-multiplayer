using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using DarkRift;

namespace TTS
{
    public class WheelSpeedMessage : TDataMessagePart
    {
        float wheelSpeed;

        public WheelSpeedMessage() { }

        public WheelSpeedMessage(float wSpeed)
        {
            this.wheelSpeed = wSpeed;
        }

        public override void Serialize(SerializeEvent e)
        {
            e.Writer.Write(wheelSpeed);
        }

        public override void Deserialize(DeserializeEvent e)
        {
            wheelSpeed = e.Reader.ReadSingle();
        }

        public override void Load(Transform target)
        {
            GameObject.FindGameObjectWithTag("Network").GetComponent<TTSClientWheelController>().SetWheelSpeed(wheelSpeed);
        }

    }
}

public class TTSTrainController : MonoBehaviour
{
    bool gameStarted = false;

    public List<string> trainCars = new List<string>(){
        "ChangingCar",
        "FridgeCar",
        "CoalCar",
        "TrainCar",
        "KitchenCar",       
        };

    public List<string> additionalLoop = new List<string>() {
        "ChangingCar",
        "TrainCar",
        "FridgeCar",
        "KitchenCar"
    };

    [HideInInspector]
    public Rigidbody rb;
    ConstantForce cf;
    public float trainEngineAcceleration = 0.0f;
    public float maxTrainEngineAcceleration = 7.0f;
    [Tooltip("Portion of trainEngineAcceleration lost each second.")]
    public float trainEngineCooldownRate = 0.2f;

    // Friction Calculation

    GameObject[] WheelAnimators;
    private float prevWheelSpeed;
    private float wheelSpeed;

    private void Awake()
    {
        rb = GameObject.Find("TrainAnchor").GetComponent<Rigidbody>();
        cf = GameObject.Find("TrainAnchor").GetComponent<ConstantForce>();
        GameObject.FindGameObjectWithTag("Network").GetComponent<TTSGeneric>().GameStarted += StartTrain;
        GetComponent<TTSID>().trackedDataSerialize += TrackedDataHandler;
    }

    void TrackedDataHandler(object sender, TTS.TrackedDataSerializeEventArgs e)
    {
    
        TTS.WheelSpeedMessage m = new TTS.WheelSpeedMessage(wheelSpeed);
        e.messages.Add(m);
  
    }

    public void BuildTrain()
    {
        Transform previousCar = gameObject.transform.Find("TrainEngine");
        Vector3 carOffset = new Vector3(0, 0, 16);
        foreach (string carPrefabID in trainCars)
        {
            GameObject carPrefab = Resources.Load($"Prefabs/{carPrefabID}", typeof(GameObject)) as GameObject;
            GameObject car = GameObject.Instantiate(carPrefab, previousCar.position + carOffset, Quaternion.identity, transform);
            car.GetComponent<TTSID>().Init();

            previousCar.Find("Colliders/Back").gameObject.SetActive(false);

            previousCar = car.transform;
        }
    }

    private void StartTrain(object sender, EventArgs e)
    {
        gameStarted = true;

        WheelAnimators = GameObject.FindGameObjectsWithTag("WheelAnimator");

        wheelSpeed = rb.velocity.magnitude / 5.45f;
        prevWheelSpeed = rb.velocity.magnitude;
        foreach (GameObject wAnim in WheelAnimators)
        {
            wAnim.GetComponent<Animator>().SetFloat("WheelSpeed", wheelSpeed);
        }
        
    }

    private void FixedUpdate()
    {
        if (gameStarted)
        {
            // Train engine cap
            if (Math.Abs(trainEngineAcceleration) > maxTrainEngineAcceleration)
                trainEngineAcceleration = Math.Sign(trainEngineAcceleration) * maxTrainEngineAcceleration;

            // Train engine cooldown
            trainEngineAcceleration *= 1 - (trainEngineCooldownRate * Time.fixedDeltaTime);

            cf.relativeForce = new Vector3(0, 0, -trainEngineAcceleration);

            wheelSpeed = rb.velocity.magnitude / 5.45f;

            //Keep Train in sync with TrainAnchor
            transform.position = rb.position;
        }
    }

    private void Update()
    {
        if (gameStarted)
        {
            foreach (GameObject wAnim in WheelAnimators)
            {
                wAnim.GetComponent<Animator>().SetFloat("WheelSpeed", wheelSpeed);
            }

            if(Mathf.Abs(prevWheelSpeed - wheelSpeed) > .05f)
            {
                GetComponent<TTSID>().trackedDataAvailable = true;
            }
        }
    }

    public void ChangeAcceleration(float change)
    {
        trainEngineAcceleration += change;
    }

    public void AddDrag(float delta)
    {
        rb.drag += delta;
    }
}
