using UnityEngine;

public class StretchToMove : MonoBehaviour
{
    [Header("The Magic Carpet")]
    [Tooltip("Drag your XR Origin here!")]
    public Transform playerRig; 

    [Header("Tracking")]
    public Transform leftTrackingHand; 
    public Transform rightTrackingHand;
    public Transform head; 

    [Header("Settings")]
    public float stretchDeadzone = 0.3f;
    public float slideSpeed = 3.0f;

    void Update()
    {
        // 1. Measure relative stretch
        float rightStretch = rightTrackingHand.position.z - head.position.z;
        float leftStretch = leftTrackingHand.position.z - head.position.z;

        // 2. MOVE THE CARPET & LOG THE ACTION
        if (rightStretch < -stretchDeadzone)
        {
            Debug.Log($"[SLIDING FORWARD] Rig Position Z: {playerRig.position.z:F2} | Right Stretch: {rightStretch:F2}");
            playerRig.Translate(Vector3.forward * slideSpeed * Time.deltaTime, Space.World);
        }
        else if (leftStretch > stretchDeadzone)
        {
            Debug.Log($"[SLIDING BACKWARD] Rig Position Z: {playerRig.position.z:F2} | Left Stretch: {leftStretch:F2}");
            playerRig.Translate(Vector3.back * slideSpeed * Time.deltaTime, Space.World);
        }
    }
}