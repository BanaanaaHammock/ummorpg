using UnityEngine;
using System.Collections;

public class CameraMMO : MonoBehaviour
{
    //Public
    public Transform target;                                // Player (Leave empty)
    public float targetHeight = 1.7f;                       // Vertical offset (Could be changed into Transform for bone focus ec "Head")
    public float distance = 12.0f;                          // Default Distance
    public float offsetCWall = 0.1f;                        // Bring camera away from any colliding objects based on layers
    public float maxDistance = 10;                          // Maximum zoom Distance
    public float minDistance = 0.1f;                        // Minimum zoom Distance
    public float xRotSpeed = 200.0f;                        // Orbit speed (Left/Right)
    public float yRotSpeed = 200.0f;                        // Orbit speed (Up/Down)
    public float yMinLimit = -80;                           // Look up limit
    public float yMaxLimit = 80;                            // Look down limit
    public float zoomRate = 40;                             // Zoom Speed
    public float rotationDampening = 3.0f;                  // Higher Value makes rot faster
    public float zoomDampening = 5.0f;                      // Higher Value makes Zoom faster

    // Collision Fix
    public LayerMask collisionLayers;                         // What layer the camera will collide with

    //Misc
    public bool lockToRearOfTarget;                         //Focus camera behind Player
    public bool allowMouseInputX = true;
    public bool allowMouseInputY = true;

    //Privates
    private float xAng = 0.0f;
    private float yAng = 0.0f;
    private float currentDistance;
    public float ActiveDistance;
    private float correctDistance;
    private bool rotateBehind;
    
    public bool inFirstPerson;                              // Change conditions to FPS

    void Start()
    {

        Vector3 angles = transform.eulerAngles;
        xAng = angles.x;
        yAng = angles.y;
        currentDistance = distance;
        ActiveDistance = distance;
        correctDistance = distance;


        if (lockToRearOfTarget)
            rotateBehind = true;
    }

    void Update()
    {
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {

            if (inFirstPerson == true)
            {

                minDistance = 0;
                ActiveDistance = 2;
                inFirstPerson = false;
            }
        }

        if (ActiveDistance <= 1)
        {

            minDistance = -1;
            ActiveDistance = -1;
            inFirstPerson = true;
        }
    }

    //After all update, move Camera
    void LateUpdate()
    {

        // Don't do anything if target is not defined
        if (!target)
            return;
        

        // If either mouse buttons are down, let the mouse govern camera position
        if (GUIUtility.hotControl == 0)
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {

            }
            else {
                if (!Utils.IsCursorOverUserInterface() && Input.GetMouseButton(1))
                {
                    //Check to see if mouse input is allowed on the axis
                    if (allowMouseInputX)
                        xAng += Input.GetAxis("Mouse X") * xRotSpeed * 0.02f;
                    if (allowMouseInputY)
                        yAng -= Input.GetAxis("Mouse Y") * yRotSpeed * 0.02f;

                }
            }
        }
        ClampAngle(yAng);

        // Camera rotation
        Quaternion rotation = Quaternion.Euler(yAng, xAng, 0);

        // Calculate the Active distance
        ActiveDistance -= Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * zoomRate * Mathf.Abs(ActiveDistance);
        ActiveDistance = Mathf.Clamp(ActiveDistance, minDistance, maxDistance);
        correctDistance = ActiveDistance;

        // Calculate desired camera position
        Vector3 vTargetOffset = new Vector3(0, -targetHeight, 0);
        Vector3 position = target.position - (rotation * Vector3.forward * ActiveDistance + vTargetOffset);

        // Check for collision based on the target's desired registration point as set by using height
        RaycastHit collisionHit;
        Vector3 trueTargetPosition = new Vector3(target.position.x, target.position.y + targetHeight, target.position.z);

        // If there is a Collision, adjust the position of the camera and calculate the correct distance
        bool isCorrected = false;
        if (Physics.Linecast(trueTargetPosition, position, out collisionHit, collisionLayers))
        {
            correctDistance = Vector3.Distance(trueTargetPosition, collisionHit.point) - offsetCWall;
            isCorrected = true;
        }

        // For smoothing, lerp distance only if either distance wasn't corrected, or correctDistance is more than ActiveDistance
        currentDistance = !isCorrected || correctDistance > currentDistance ? Mathf.Lerp(currentDistance, correctDistance, Time.deltaTime * zoomDampening) : correctDistance;

        // Keep the limits
        currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);

        // Recalculate position based on the new currentDistance
        position = target.position - (rotation * Vector3.forward * currentDistance + vTargetOffset);

        //Finally Set rotation and position of camera
        transform.rotation = rotation;
        transform.position = position;
    }

    void ClampAngle(float angle)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;

        yAng = Mathf.Clamp(angle, -60, 80);
    }

}