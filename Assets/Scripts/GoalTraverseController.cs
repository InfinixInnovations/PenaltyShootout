using UnityEngine;

public class GoalTraverseController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The Main Camera tracking the headset")]
    public Transform headsetCamera;

    [Tooltip("The XR Rig that should be moved sideways")]
    public Transform xrRig;

    [Header("Settings")]
    [Tooltip("How much to amplify physical movement. 1 means 1:1 extra movement (Total 2x). 2 means 2:1 extra (Total 3x)")]
    public float amplificationFactor = 2.0f;
    
    [Tooltip("The minimum Z coordinate for the left post limit")]
    public float limitMinZ = 1.8f;
    [Tooltip("The maximum Z coordinate for the right post limit")]
    public float limitMaxZ = 8.52f;

    private Vector3 lastHeadsetLocalPos;

    void Start()
    {
        if (headsetCamera == null)
        {
            headsetCamera = Camera.main != null ? Camera.main.transform : null;
        }

        if (headsetCamera != null)
        {
            lastHeadsetLocalPos = headsetCamera.localPosition;
        }
        else
        {
            Debug.LogWarning("GoalTraverseController: No headset camera assigned or found!");
        }

        if (xrRig == null)
        {
            Debug.LogWarning("GoalTraverseController: No XR Rig assigned!");
        }
    }

    void Update()
    {
        if (headsetCamera == null || xrRig == null) return;

        Vector3 currentLocalPos = headsetCamera.localPosition;
        
        // Calculate the physical movement in Tracking Space (IRL X, Y, Z)
        Vector3 localDelta = currentLocalPos - lastHeadsetLocalPos;

        // The user specifically wants their IRL X-axis mapped to the game's World Z-axis (the goal line)
        float sidewaysDelta = localDelta.x;

        if (Mathf.Abs(sidewaysDelta) > 0.0001f)
        {
            float extraMovementZ = sidewaysDelta * amplificationFactor;

            Vector3 newPosition = xrRig.position;
            newPosition.z += extraMovementZ;

            // Clamp the World Z position to stay within the goal posts
            newPosition.z = Mathf.Clamp(newPosition.z, limitMinZ, limitMaxZ);

            xrRig.position = newPosition;
        }

        // Update last position to the *new* local position
        lastHeadsetLocalPos = headsetCamera.localPosition;
    }
}
