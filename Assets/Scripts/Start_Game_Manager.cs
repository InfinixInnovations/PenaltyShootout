using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Start_Game_Manager : MonoBehaviour
{
    [Header("Tracking References")]
    public FullSkeletonTracker playerTracker;
    [Tooltip("The exact tdJointName used for the hand (e.g., p1/joint_hand_r or hand_r)")]
    public string handJointName = "hand_r";
    [Tooltip("The exact tdJointName used for the shoulder (e.g., shoulder_r)")]
    public string shoulderJointName = "shoulder_r";


    [Header("UI Feedback")]
    public Slider handProgressSlider;
    public GameObject startUIGroup; // Parent object holding the slider and "Raise Hand" text

    [Header("Game UI")]
    public GameObject ingameUI;
    public GameObject avatar;
    public GameObject ball;

    [Header("Activation Rules")]
    [Tooltip("How many meters ABOVE the shoulder the hand needs to go to fully register")]
    public float targetHeightAboveShoulder = 0.4f;
    [Tooltip("How many seconds they must hold their hand up to start the game")]
    public float requiredHoldTime = 2f;

    private float currentHoldTimer = 0f;
    private bool triggerActivated = false;

    void Start()
    {
        if (handProgressSlider != null) handProgressSlider.value = 0f;
        if (startUIGroup != null) startUIGroup.SetActive(true);

        ingameUI.SetActive(false);
        avatar.SetActive(false);
        ball.SetActive(false);
    }

    void Update()
    {
        if (triggerActivated || playerTracker == null) return;

        Transform handTransform = GetTrackerJoint(handJointName);
        Transform shoulderTransform = GetTrackerJoint(shoulderJointName);

        if (handTransform != null && shoulderTransform != null)
        {
            // FIX: If the joints are stuck at exactly (0,0,0), it means nobody is in frame yet!
            if (handTransform.position == Vector3.zero && shoulderTransform.position == Vector3.zero)
            {
                ResetStartTrigger();
                return;
            }

            // Calculate how much higher the hand is than the shoulder on the Y axis
            float currentHeightDiff = handTransform.position.y - shoulderTransform.position.y;

            // Map that height difference directly to a 0 to 1 percentage loop
            float progressPercentage = Mathf.Clamp01(currentHeightDiff / targetHeightAboveShoulder);

            if (handProgressSlider != null)
            {
                handProgressSlider.value = progressPercentage;
            }

            // If they have pushed their hand past the threshold height, count down the timer
            if (progressPercentage >= 1f)
            {
                currentHoldTimer += Time.deltaTime;

                if (currentHoldTimer >= requiredHoldTime)
                {
                    StartGameSession();

                }
            }
            else
            {
                // If they drop their hand early, the meter slowly drains down
                currentHoldTimer = Mathf.Max(0f, currentHoldTimer - Time.deltaTime * 2f);
            }
        }
        else
        {
            ResetStartTrigger();
        }
    }

    // Helper function to keep code clean and dry
    private void ResetStartTrigger()
    {
        if (handProgressSlider != null) handProgressSlider.value = 0f;
        currentHoldTimer = 0f;
    }

    private void StartGameSession()
    {
        triggerActivated = true;
        // 1. Hide the hand calibration progress UI background and slider instantly
        if (startUIGroup != null) startUIGroup.SetActive(false);
        ingameUI.SetActive(true);
        //avatar.SetActive(true);
        ball.SetActive(true);
    }

    private Transform GetTrackerJoint(string nameID)
    {
        foreach (var joint in playerTracker.skeletonJoints)
        {
            if (joint.tdJointName == nameID) return joint.markerTransform;
        }
        return null;
    }
}