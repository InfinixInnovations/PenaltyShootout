using UnityEngine;
using UnityEngine.XR;

public class VRRecenter : MonoBehaviour
{
    [Tooltip("The root XR Rig or XR Origin to move")]
    public Transform xrRig;
    
    [Tooltip("The Main Camera tracking the headset")]
    public Transform headsetCamera;

    [Header("Target Location")]
    [Tooltip("The world position where the player should be centered")]
    public Vector3 targetPosition = new Vector3(-41.19f, 5.79f, 5.16f);
    
    [Tooltip("The direction the player should be facing when recentered")]
    public Vector3 targetForward = Vector3.right; // Looking down +X axis towards the cannon

    [Header("Input Settings")]
    [Tooltip("Which controller to look for input on")]
    public XRNode controllerNode = XRNode.LeftHand;
    
    [Tooltip("How long you must hold the button to trigger recenter")]
    public float holdTimeToRecenter = 1.5f;

    private float holdTimer = 0f;

    void Start()
    {
        if (headsetCamera == null && Camera.main != null)
        {
            headsetCamera = Camera.main.transform;
        }
    }

    void Update()
    {
        if (xrRig == null || headsetCamera == null) return;

        InputDevice device = InputDevices.GetDeviceAtXRNode(controllerNode);
        bool buttonPressed = false;

        // Listen for the Menu button or the Primary button (X / A)
        if (device.TryGetFeatureValue(CommonUsages.menuButton, out bool menuPressed) && menuPressed)
        {
            buttonPressed = true;
        }
        else if (device.TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryPressed) && primaryPressed)
        {
            buttonPressed = true;
        }

        if (buttonPressed)
        {
            holdTimer += Time.deltaTime;
            if (holdTimer >= holdTimeToRecenter)
            {
                Recenter();
                holdTimer = 0f; // Reset to prevent continuous rapid recentering
            }
        }
        else
        {
            holdTimer = 0f; // Reset if button is let go early
        }
    }

    public void Recenter()
    {
        // 1. Calculate Rotation Correction
        // We want the headset's forward vector (flattened to the floor) to match targetForward.
        Vector3 headForward = Vector3.ProjectOnPlane(headsetCamera.forward, Vector3.up).normalized;
        
        // Find the angle difference
        float angleDiff = Vector3.SignedAngle(headForward, targetForward, Vector3.up);
        
        // Rotate the entire rig around the headset's current position so the headset looks the right way
        xrRig.RotateAround(headsetCamera.position, Vector3.up, angleDiff);

        // 2. Calculate Position Correction
        // We want the headset's position (flattened to the floor) to match the targetPosition's X and Z.
        // We do NOT modify Y, so we don't mess up the player's physical height tracking.
        Vector3 headPosXZ = new Vector3(headsetCamera.position.x, 0, headsetCamera.position.z);
        Vector3 targetPosXZ = new Vector3(targetPosition.x, 0, targetPosition.z);
        
        Vector3 positionOffset = targetPosXZ - headPosXZ;
        
        // Slide the rig by the exact offset needed to center the headset
        xrRig.position += positionOffset;

        Debug.Log("VR View Recentered! Look towards the cannon.");
    }
}
