using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
/*
 * CameraTracker Unity Script
 * - Add script to camera you want to have track player
 * - Tracks which car the local player is in
 * - Smoothly interpolates camera between cars to always display car player is in
 */
public class CameraTracker : MonoBehaviour
{
    private Vector3 targetPos = new Vector3(-16.64f, 12.27f, 4.65f);
    private Quaternion targetRotation = Quaternion.Euler(12, 100, 0);

    Transform localPlayer = null;

    private void Awake()
    {
        GameObject.FindGameObjectWithTag("Network")
            .GetComponent<TTSGeneric>().GameStarted += GameStarted;
    }

    public void GameStarted(object sender, EventArgs e)
    {
        localPlayer = GameObject.FindGameObjectWithTag("Network")
            .GetComponent<TTSGeneric>().GetLocalPlayer();
    }

    void Update()
    {
        if(localPlayer != null)
        {
            Transform currentTrainCar = localPlayer.GetComponent<TTSNetworkedPlayer>().currentTrainCar;
            transform.parent = currentTrainCar;
            Transform cameraAnchor = currentTrainCar.Find("CameraAnchor");
            targetPos = cameraAnchor.position;
            targetRotation = cameraAnchor.rotation;

            if (transform.position != targetPos)
            {
                transform.position = Vector3.Lerp(transform.position, targetPos, .05f);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, .1f);
                if (Vector3.Distance(transform.position, targetPos) < .1f)
                {
                    transform.position = targetPos;
                    transform.rotation = targetRotation;
                }
            }

        }      


    }
}
