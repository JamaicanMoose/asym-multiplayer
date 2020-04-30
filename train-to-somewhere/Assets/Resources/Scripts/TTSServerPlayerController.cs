using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TTSServerPlayerController : MonoBehaviour
{
    bool gameStarted = false;

    bool dashing = false;
    bool lastDashing = false;
    Vector3 lastMoveVector = Vector3.zero;
    Transform mainCameraTransform;
    TTSServer ttsserver;
    TTSNetworkedPlayer localPlayer;

    private void Awake()
    {
        ttsserver = GameObject.FindGameObjectWithTag("Network").GetComponent<TTSServer>();
        mainCameraTransform = GameObject.FindWithTag("MainCamera").transform;
        ttsserver.GameStarted += GameStarted;
    }

    private void GameStarted(object sender, EventArgs e)
    {
        gameStarted = true;
        localPlayer = ttsserver.GetLocalPlayer().gameObject.GetComponent<TTSNetworkedPlayer>();
    }

    void Start()
    {
        
    }

    void Update()
    {
        HandleLocalInput();
    }

    private void HandleLocalInput()
    {
        Vector3 moveDirection = mainCameraTransform.right * Input.GetAxis("Horizontal") + mainCameraTransform.forward * Input.GetAxis("Vertical");

        moveDirection.y = 0f;
        moveDirection.Normalize();

        dashing = Input.GetKey(KeyCode.Space);

        if (moveDirection != lastMoveVector || dashing != lastDashing)
        {
            localPlayer.SetVelocity(moveDirection, dashing);
        }

        lastMoveVector = moveDirection;
        lastDashing = dashing;
    }
}
