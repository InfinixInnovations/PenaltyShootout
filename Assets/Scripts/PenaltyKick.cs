using NUnit.Framework.Internal.Commands;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class PenaltyKick : MonoBehaviour
{
    [Header("Goal Setup")]
    [Tooltip("Empty GameObject placed at the center of the goal mouth.")]
    public Transform goalCenter;

    [Tooltip("Total width of the goal in meters. Real goal is ~7.32m.")]
    public float goalWidth = 7.32f;

    [Tooltip("Height at which the ball arrives at the goal line.")]
    public float targetHeight = 1.2f;

    [Tooltip("Inset from the posts so shots don't snap onto the woodwork.")]
    public float postInset = 0.6f;

    [Header("Arc")]
    [Tooltip("Time the ball spends in scripted flight, in seconds.")]
    public float flightDuration = 0.7f;

    [Tooltip("Peak height of the arc above the midpoint of start->target.")]
    public float arcHeight = 1.5f;

    [Header("Detection")]
    [Tooltip("Substring matched against collider GameObject names to identify feet.")]
    public string footTag = "Foot_Collider_Container";

    [Tooltip("Minimum horizontal foot velocity to register as a kick (m/s).")]
    public float minKickSpeed = 0.5f;

    [Header("Residual Physics")]
    [Tooltip("Speed given to the ball after the arc ends, so it bounces in the net naturally.")]
    public float residualSpeed = 2f;

    [Header("Reset")]
    public float resetTimer = 5f;
    public GameObject ballPosition;
    Vector3 originPos;

    [Header("Slider")]
    public Slider slider;
    public float sliderStepSize = 50f;

    [Header("Goal Detection")]
    public Goal_Handler goalDetector;

    Rigidbody rb;
     bool kicked;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        originPos = ballPosition.transform.position;
        Debug.Log(originPos);
    }

    void OnCollisionEnter(Collision col)
    {
        if (kicked) return;
        if (!col.gameObject.name.Contains(footTag)) return;

        Vector3 footVel = GetFootVelocity(col);
        Debug.Log("Foot velocity:" + footVel);

        // Ignore feather-touches
        Vector3 horiz = new Vector3(footVel.x, 0f, footVel.z);
        if (horiz.magnitude < minKickSpeed) return;

        float lockedPower = slider.value;

        if (lockedPower < 50f)
        {
            // Add any behaviour for when shot is underpowered
            Debug.Log("Ball UNDERSHOT");
            return;
        }

        Vector3 target = PickTarget(horiz);

        if (lockedPower > 75f)
        {
            target = ApplyOvershoot(target, lockedPower);
            Debug.Log("Ball OVERSHTO");
        }


        kicked = true;

        // Only called for when a shot misses otherwise Goal_Handler handles success
        if (goalDetector != null)
            goalDetector.RegisterShot();

        StartCoroutine(FlyToTarget(target));
        StartCoroutine(CountdownToReset(resetTimer));
    }

    private void Update()
    {
        if (!kicked)
        ChangeSliderValue();
    }

    Vector3 GetFootVelocity(Collision col)
    {
        //if (col.rigidbody != null)
        //    return col.rigidbody.linearVelocity;

        var tracker = col.gameObject.GetComponent<FootVelocityTracker>();
        if (tracker != null)
            return tracker.Velocity;

        // Last resort: use the contact's relative velocity as an approximation.
        return -col.relativeVelocity;
    }

    Vector3 PickTarget(Vector3 horizFootVelocity)
    {
        if (goalCenter == null)
        {
            Debug.LogWarning("PenaltyKick: goalCenter not assigned. Defaulting to world forward.");
            return transform.position + transform.forward * 10f;
        }

        // How far across the goal (left = -1, right = +1) is the foot aiming?
        float lateral = Vector3.Dot(horizFootVelocity.normalized, goalCenter.right);

        // Bucket into 3 zones with generous deadzones so noisy input still snaps cleanly.
        float zoneOffset;
        if (lateral < -0.25f) zoneOffset = -(goalWidth * 0.5f - postInset); // left
        else if (lateral > 0.25f) zoneOffset = (goalWidth * 0.5f - postInset); // right
        else zoneOffset = 0f;                              // middle

        Vector3 target = goalCenter.position
                       + goalCenter.right * zoneOffset
                       + Vector3.up * targetHeight;
        return target;
    }

    IEnumerator FlyToTarget(Vector3 target)
    {
        // Freeze physics so nothing else perturbs the ball mid-flight.
        bool wasKinematic = rb.isKinematic;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        Vector3 start = transform.position;
        Vector3 flightDir = (target - start).normalized;

        Vector3 spinAxis = Vector3.Cross(Vector3.up, flightDir);

        float elapsed = 0f;
        while (elapsed < flightDuration)
        {
            float t = elapsed / flightDuration;
            Vector3 linear = Vector3.Lerp(start, target, t);
            // Parabola: 4h * t * (1-t) peaks at t=0.5 with value arcHeight.
            linear.y += 4f * arcHeight * t * (1f - t);
            transform.position = linear;
            transform.Rotate(spinAxis, 720f * Time.deltaTime, Space.World);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = target;

        // Hand control back to the physics engine so the ball can bounce around in the net.
        rb.isKinematic = wasKinematic;
        rb.linearVelocity = flightDir * residualSpeed;

        yield return new WaitForSeconds(0.5f);
        kicked = false;
    }

    IEnumerator CountdownToReset(float delay)
    {

        yield return new WaitForSeconds(delay);
        ResetBall(originPos);

    }

    private Vector3 ApplyOvershoot(Vector3 orgTarget, float pow)
    {
        Vector3 dirToGoal = orgTarget - transform.position;
        dirToGoal.y = 0;
        Vector3 forward = dirToGoal.normalized;

        float overshootPow = Mathf.InverseLerp(75f, 99f, pow);

        float extraDistance = Mathf.Lerp(2f, 8f, overshootPow);
        float extraHeight = Mathf.Lerp(1.5f, 4f, overshootPow);

        return orgTarget + (forward * extraDistance) + (Vector3.up * extraHeight);
    }

    public void ChangeSliderValue()
    {
        float pingPongValue = Mathf.PingPong(Time.time * sliderStepSize, 99f);
        slider.value  = pingPongValue;
    }

    public void ResetBall(Vector3 position)
    {
        StopAllCoroutines();
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = position;
        kicked = false;
        rb.isKinematic = false;
    }

    public void ClickResetBallPos()
    {
        ResetBall(originPos);
    }
}