using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainCarTracker: MonoBehaviour
{
    Transform mainCameraTransform;

    //Position of camera in train car space
    Vector3 cameraLocalPos = new Vector3(-16.64f, 9.37f, 4.65f);
    Quaternion cameraLocalRotation = Quaternion.Euler(12, 100, 0);

    private Vector3 targetPos;

    // Start is called before the first frame update
    void Start()
    {
     
        mainCameraTransform = GameObject.FindGameObjectWithTag("MainCamera").transform;

        Ray down = new Ray(gameObject.transform.position, Vector3.down);
        RaycastHit hit;
        if (Physics.Raycast(down, out hit))
        {
            if (hit.collider.name == "Floor")
            {
                Transform trainCarTransform = hit.collider.transform.parent.parent;
                Transform cameraAnchorTransform = trainCarTransform.Find("CameraAnchor");
               
                targetPos = cameraAnchorTransform.position;
         

            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(mainCameraTransform.position != targetPos)
        {
            mainCameraTransform.position = Vector3.Lerp(mainCameraTransform.position, targetPos, .05f);
            if (Vector3.Distance(mainCameraTransform.position, targetPos) < .1f)
                mainCameraTransform.position = targetPos;
        }
        else
        {
            // Find out which car we are in.
            Ray down = new Ray(gameObject.transform.position, Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(down, out hit))
            {
                if(hit.collider.name == "Floor")
                {
                    Transform trainCarTransform = hit.collider.transform.parent.parent;
                    Transform cameraAnchorTransform = trainCarTransform.Find("CameraAnchor");
                    // Calculate camera position & rotation
                    targetPos = cameraAnchorTransform.position;
                    //mainCameraTransform.SetPositionAndRotation(cameraAnchorTransform.position, cameraAnchorTransform.rotation);
                    // Reparent player
                    gameObject.transform.parent = trainCarTransform;

                }
            }
        }
  
    }

  
}
