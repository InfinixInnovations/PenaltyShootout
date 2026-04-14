using UnityEngine;
using System.Collections;

public class BallShooter : MonoBehaviour
{
    public Rigidbody ballRb;

    [Header("Shot Settings")]
    public float shotForceMultiplier = 0.4f;
    public float kickDelay = 2f;

    [Header("Arc Control")]
    public float minHeight = 0.1f;
    public float maxHeight = 0.4f;

    [Header("Overshoot")]
    public float overshootThreshold = 90f;
    public float overshootLift = 0.3f;

    [Header("Reset")]
    public float resetDelay = 3f;

    [Header("References")]
    public TargetSelector targetSelector;
    public Transform player;

    private Vector3 ballStartPos;
    private Quaternion ballStartRot;

    private Vector3 playerStartPos;
    private Quaternion playerStartRot;

    void Start()
    {
        // Store initial positions
        ballStartPos = ballRb.transform.position;
        ballStartRot = ballRb.transform.rotation;

        playerStartPos = player.position;
        playerStartRot = player.rotation;
    }

    public void Shoot(Transform target, float power)
    {
        StartCoroutine(ShootRoutine(target, power));
    }

    IEnumerator ShootRoutine(Transform target, float power)
    {
        // ⏳ Wait for animation timing
        yield return new WaitForSeconds(kickDelay);

        // Reset motion
        ballRb.linearVelocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;

        // Direction
        Vector3 rawDirection = target.position - ballRb.position;
        Vector3 horizontalDir = new Vector3(rawDirection.x, 0f, rawDirection.z).normalized;

        float heightFactor = Mathf.Lerp(minHeight, maxHeight, power / 100f);

        Vector3 finalDirection = horizontalDir + Vector3.up * heightFactor;

        if (power >= overshootThreshold)
        {
            finalDirection += Vector3.up * overshootLift;
        }

        finalDirection.Normalize();

        // Shoot
        ballRb.AddForce(finalDirection * power * shotForceMultiplier, ForceMode.Impulse);

        // ⏳ Wait before reset
        yield return new WaitForSeconds(resetDelay);

        ResetScene();
        targetSelector.ResetToAiming();
    }

    void ResetScene()
    {
        // Reset ball
        ballRb.linearVelocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;
        ballRb.transform.position = ballStartPos;
        ballRb.transform.rotation = ballStartRot;

        // Reset player position
        player.position = playerStartPos;
        player.rotation = playerStartRot;
    }
}