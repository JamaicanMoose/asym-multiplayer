using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TTSServerPlayerController : MonoBehaviour
{
    bool gameStarted = false;

    bool dashing = false;
    bool lastDashing = false;

    bool fire1D = false;
    bool fire1Prev = false;

    bool fire2D = false;
    bool fire2Prev = false;

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
        if(gameStarted)
        {
            HandleLocalInput();
        }
        
    }

    private void HandleLocalInput()
    {
        Vector3 moveDirection = mainCameraTransform.right * Input.GetAxis("Horizontal") + mainCameraTransform.forward * Input.GetAxis("Vertical");

        moveDirection.y = 0f;
        moveDirection.Normalize();

        dashing = Input.GetButton("Jump");

        bool newInput = false;

        if(Input.GetButtonDown("Fire1"))
        {
            fire1D = true;
            fire1Prev = false;
            newInput = true;
        }
        else if(Input.GetButtonUp("Fire1"))
        {
            fire1D = false;
            fire1Prev = true;
            newInput = true;
        }

        if (Input.GetButtonDown("Fire2"))
        {
            fire2D = true;
            fire2Prev = false;
            newInput = true;
        }
        else if (Input.GetButtonUp("Fire2"))
        {
            fire2D = false;
            fire2Prev = true;
            newInput = true;
        }  

        if (moveDirection != lastMoveVector || dashing != lastDashing)
        {
            localPlayer.SetMovementInput(moveDirection, dashing);
        }

        if(newInput)
        {
            localPlayer.SetFireInput(fire1D, fire1Prev, fire2D, fire2Prev);            
        }
      

        lastMoveVector = moveDirection;
        lastDashing = dashing;
       
    }
}
