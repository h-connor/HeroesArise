using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof (Camera))]
public class CameraBehaviour : MonoBehaviour
{
    const string MAIN_CAM_TAG = "MainCamera";
    const string NO_TAG = "Untagged";
    public float FOLLOW_SPEED = 18.5f; // Speed at which camera follows center
    public float LOOK_SPEED = 3.5f; // Speed at which camera turns to look at center
    public float USER_CONTROL_SPEED = 2.7f; // Speed at which camera is moved by player

    // How many pixels the player can move the camera from horizontally / vertically
    const float MAX_HORIZONTAL_OFFSET = 20f;
    const float MAX_VERTICAL_OFFSET = 13f;

    Camera cam;

    // Initial position of camera
    Vector3 initialPos;
    Vector3 initialDist = Vector3.zero;

    // Moving the camera to the following positions
    float moveCamToY;
    float moveCamToX;

    // Is the player moving the camera?
    bool movingHorizontally = false;
    bool movingVert = false;

    // Scrollbars to move the camera
    [SerializeField]
    Scrollbar verticalScroll = null;
    [SerializeField]
    Scrollbar horizontalScroll = null;

    [SerializeField]
    Transform cameraCenter = null;

    [SerializeField]
    Camera bossCam = null;

    // The position of the cameraCenter, that the camera is following
    // Update this every frame, and offset it by the last follow to determine how far to move the camera
    Vector3 centerPos;

    void Awake()
    {
        cam = this.GetComponent<Camera>();
        centerPos = cameraCenter.position;
        initialPos = cam.transform.position;
        moveCamToY = initialPos.y;
        moveCamToX = initialPos.x;

        if (cameraCenter == null || verticalScroll == null || horizontalScroll == null || bossCam == null)
        {
            throw new ElementNotDefined("Error, Scroll bars or camera not properly initialized.");
        }

        if (verticalScroll.onValueChanged.GetPersistentEventCount () == 0 || horizontalScroll.onValueChanged.GetPersistentEventCount () == 0)
        {
            throw new ElementNotDefined("Error, Scroll bars do not have a function attached.");
        }

        initialDist = cameraCenter.position - cam.transform.position;
    }

    /// <summary>
    /// Move the two cameras according to the appropriate positions
    /// </summary>
    void FixedUpdate()
    {
        LookAt(cam.transform, cameraCenter.position);
        LookAt(bossCam.transform, cameraCenter.position);
        MoveCam(cam.transform);
        MoveCam(bossCam.transform);
    }

    // Ensure the camera is looking towards the specified position
    // Speed is given by the lookSpeed 
    void LookAt (Transform obj, Vector3 pos)
    {
        Quaternion rotation = Quaternion.LookRotation(pos - obj.position);
        obj.rotation = Quaternion.Slerp(obj.rotation, rotation, Time.deltaTime * LOOK_SPEED);
    }

    // Ensure the camera is following the given position
    // Utilizing the followSpeed
    // The camera will move based on the offset of the last frame that the position was moved
    void MoveWithCenter()  
    {
            Vector3 offset = (cameraCenter.position - centerPos);

            // Moving camera based on offset of where the center moved
            cam.transform.position += offset; // Vector3.Lerp(cam.transform.position, cam.transform.position + offset, Time.deltaTime * this.FOLLOW_SPEED);
            bossCam.transform.position += offset;
            initialPos += offset;

            centerPos = cameraCenter.position;
    }

    // Move the transform (camera) to the position specified based on the user controlled scroll bars
    // If a scroll bar was previously modified, it will lerp towards the point specified
    // Once reached, the function will no longer run anything for optimization purposes
    // NOTE this function is coupled with MoveCamera functions
    void MoveCam (Transform pos)
    {
        if (movingVert && Mathf.Abs(pos.position.y - moveCamToY) > Mathf.Epsilon)
        {
            cam.transform.position = Vector3.Lerp(cam.transform.position,
                                    new Vector3(cam.transform.position.x, moveCamToY, cam.transform.position.z),
                                    Time.deltaTime * USER_CONTROL_SPEED);
        }
        else
            movingVert = false;

        if (movingHorizontally && Mathf.Abs(pos.position.x - moveCamToX) > Mathf.Epsilon)
        {
            cam.transform.position = Vector3.Lerp(cam.transform.position,
                                     new Vector3(moveCamToX, cam.transform.position.y, cam.transform.position.z),
                                     Time.deltaTime * USER_CONTROL_SPEED);
        }
        else
            movingHorizontally = false;
    }

    // The player has moved the camera on the vertical axis
    // if the scroll is half way (0.5) then the camera is in default position
    public void MoveCameraVertically ()
    {
        float scrollVal = verticalScroll.value;
        float upMostY = initialPos.y + MAX_VERTICAL_OFFSET;
        float downMostY = initialPos.y - MAX_VERTICAL_OFFSET;

        moveCamToY = (scrollVal * (upMostY - downMostY)) + downMostY;
        movingVert = true;
    }

    // The player has moved the camera on the horizontal axis
    // if the scroll is half way (0.5) then the camera is in default position
    public void MoveCameraHorizontally ()
    {
        float scrollVal = horizontalScroll.value;
        float rightMostX = initialPos.x + MAX_HORIZONTAL_OFFSET;
        float leftMostX = initialPos.x - MAX_HORIZONTAL_OFFSET;
        
        moveCamToX = (scrollVal * (rightMostX - leftMostX)) + leftMostX;
        movingHorizontally = true;
    }

    // Move the center for the camera based on the following positions
    // The center will move to the average of all of the positions, based on the X and Z axis
    public void MoveCenterByPos (Vector3 [] positions)
    {
        // For the average of the x and z coordinates of the characters
        if (positions != null && positions.Length > 0)
        {
            float sumX = positions[0].x;
            float sumZ = positions[0].z; 

            for (int i = 1; i < positions.Length; i++)
            {
                sumX += positions[i].x;
                sumZ += positions[i].z;
            }

            // Set the center to the average
            cameraCenter.position = new Vector3(sumX / positions.Length, cameraCenter.position.y, sumZ / positions.Length);

            // Move the camera whenever center gets updated
            MoveWithCenter();
        }
        else
        {
            throw new FightConfigError("Error, camera setup failure.");
        }
    }

    /// <summary>
    /// Switches between the boss and wave cameras
    /// </summary>
    public void SwitchCameras ()
    {
        if (bossCam == null)
            throw new ElementNotDefined("Error, other camera not defined.");

        if (bossCam == Camera.main)
        {
            bossCam.GetComponent<Camera>().enabled = false;
            bossCam.tag = NO_TAG;
            this.GetComponent<Camera>().enabled = true;
            this.tag = MAIN_CAM_TAG;
        }
        else
        {
            bossCam.GetComponent<Camera>().enabled = true;
            bossCam.tag = MAIN_CAM_TAG;
            this.GetComponent<Camera>().enabled = false;
            this.tag = NO_TAG;
        }
    }
}
