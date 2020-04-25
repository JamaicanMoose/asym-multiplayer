using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainCarTracker: MonoBehaviour
{
    Transform mainCameraTransform;

    //Position of camera in train car space
    //Vector3 cameraLocalPos = new Vector3(-16.64f, 9.37f, 4.65f);
    //Quaternion cameraLocalRotation = Quaternion.Euler(12, 100, 0);

    private Vector3 targetPos = new Vector3(-16.64f, 12.27f, 4.65f);
    private Quaternion targetRotation = Quaternion.Euler(12, 100, 0);

    private LocalPlayerController playerController;
    private PlayerObject playerObj;
    // Start is called before the first frame update
    void Start()
    {    
        mainCameraTransform = GameObject.FindGameObjectWithTag("MainCamera").transform;
        mainCameraTransform.position = targetPos;
        mainCameraTransform.rotation = targetRotation;

        playerController = GetComponent<LocalPlayerController>();
        playerObj = GetComponent<PlayerObject>();
    }

    // Update is called once per frame
    void Update()
    {
        Ray down = new Ray(gameObject.transform.position, Vector3.down);
        RaycastHit hit;
        if (Physics.Raycast(down, out hit))
        {
            if (hit.collider.name == "Floor")
            {
                Transform trainCarTransform = hit.collider.transform.parent.parent;

                Transform cameraAnchorTransform = trainCarTransform.Find("CameraAnchor");

                // Calculate camera position & rotation
                targetPos = cameraAnchorTransform.position;
                targetRotation = cameraAnchorTransform.rotation;

                // Reparent player
                Vector3 playerWorldPosition = transform.position;
                gameObject.transform.parent = trainCarTransform;
                playerObj.SetMovePosition(trainCarTransform.InverseTransformPoint(playerWorldPosition));
                playerController.parentCarID = trainCarTransform.GetComponent<NetworkTrackable>().uniqueID;

            }
        }

        if (mainCameraTransform.position != targetPos)
        {          
            mainCameraTransform.position = Vector3.Lerp(mainCameraTransform.position, targetPos, .05f);
            mainCameraTransform.rotation = Quaternion.Lerp(mainCameraTransform.rotation, targetRotation, .1f);
            if (Vector3.Distance(mainCameraTransform.position, targetPos) < .1f)
            {
                mainCameraTransform.position = targetPos;
                mainCameraTransform.rotation = targetRotation;
            }        
        }
       
  
    }

  
}
