using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;

namespace TTS {
    public class NetworkedPlayerSyncMessage : GODataSyncMessage
    {
        float foodLevel;

        public NetworkedPlayerSyncMessage() { }

        public NetworkedPlayerSyncMessage(float foodLevel)
        {
            this.foodLevel = foodLevel;
        }

        public override void Serialize(SerializeEvent e)
        {
            e.Writer.Write(foodLevel);
        }

        public override void Deserialize(DeserializeEvent e)
        {
            foodLevel = e.Reader.ReadSingle();
        }

        public override void Load(Transform target)
        {
            target.GetComponent<TTSNetworkedPlayer>().foodLevel = foodLevel;
        }
    }
}

public class TTSNetworkedPlayer : MonoBehaviour
{
    private Rigidbody rb;
    private bool trackedDataAvailable = false;

    public float moveSpeed = 2.5f;
    public float dashSpeed = 20f;
    public float dashTime = .3f;

    bool prevdashButtonDown = false;
    bool dashButtonDown = false;

    Vector3 currentMoveVector = Vector3.zero;

    bool dashing = false;

    public float dashFoodCost = .1f;
    public float foodLevel = 100f;

    public Vector3 lastSyncPostion;
    private Vector3 lastMoveVector;

    private void Awake()
    {
        lastSyncPostion = transform.localPosition;
        lastMoveVector = Vector3.zero;
        rb = GetComponent<Rigidbody>();
        GetComponent<TTSID>().trackedDataSerialize += TrackedDataHandler;
    }
    // Update is called once per frame
    void Update()
    {
        bool onDashButtonDown = dashButtonDown && !prevdashButtonDown;

        if (!dashing && onDashButtonDown && currentMoveVector != Vector3.zero)
        {
            float costPerDash = dashFoodCost * (dashTime / Time.deltaTime);
            if (foodLevel >= costPerDash)
            {
                dashing = true;
                StartCoroutine(DashTimer());
            } else
            {
                GetComponentInChildren<MeshRenderer>().material.color = Color.red;
            }
        }
        if (dashing)
        {
            foodLevel -= dashFoodCost;
            trackedDataAvailable = true;
            GetComponent<TTSID>().trackedDataAvailable = true;
            rb.velocity = currentMoveVector * dashSpeed;
        } else
        {
            rb.velocity = currentMoveVector * moveSpeed;
        }

        prevdashButtonDown = dashButtonDown;

        lastMoveVector = currentMoveVector;

        if (transform.forward.normalized != lastMoveVector.normalized)
        {
            transform.forward = Vector3.RotateTowards(transform.forward, lastMoveVector, 10 * Time.deltaTime, 0.0f);
            Debug.DrawRay(transform.position, lastMoveVector, Color.red);
        }
    }

    void TrackedDataHandler(object sender, TTS.TrackedDataSerializeEventArgs e)
    {
        if (trackedDataAvailable)
        {
            TTS.NetworkedPlayerSyncMessage m = new TTS.NetworkedPlayerSyncMessage(foodLevel);
            e.messages.Add(m);
        }
    }

    IEnumerator DashTimer()
    {
        yield return new WaitForSeconds(dashTime);
        dashing = false;
    }

    public void SetMovementInput(Vector3 direction, bool dashButton)
    {
        dashButtonDown = dashButton;
        currentMoveVector = direction;
    }


}
