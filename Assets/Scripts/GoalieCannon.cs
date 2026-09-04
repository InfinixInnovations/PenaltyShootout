using System.Collections;
using UnityEngine;

public class GoalieCannon : MonoBehaviour
{
    [Header("Cannon Parts")]
    public Transform cannonShaft; 
    public Transform firePoint;
    public GameObject ballPrefab;

    [Header("Visuals & Audio")]
    [Tooltip("Drag a Point Light or glowing object here")]
    public GameObject warningHighlight; 
    public ParticleSystem fireParticles;
    public AudioSource fireAudio;

    public void ToggleHighlight(bool isOn)
    {
        if (warningHighlight != null)
        {
            warningHighlight.SetActive(isOn);
        }
    }

    public void AimAndShoot(Vector3 targetPoint, float flightTime)
    {
        if (cannonShaft != null) cannonShaft.LookAt(targetPoint);

        if (ballPrefab != null && firePoint != null)
        {
            if (fireParticles != null) fireParticles.Play();
            if (fireAudio != null) fireAudio.Play();

            // 1. Spawn the ball (Declared exactly once!)
            GameObject newBall = Instantiate(ballPrefab, firePoint.position, firePoint.rotation);
            newBall.tag = "Ball"; // Explicitly set the tag so the ScoreTrigger recognizes it!
            
            // Add BallBehavior to handle save logic and bounce sounds
            newBall.AddComponent<BallBehavior>();
            
            // 2. Tell the ScoreManager we fired a shot!
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddShot();
            }

            Rigidbody rb = newBall.GetComponent<Rigidbody>();
            Collider ballCollider = newBall.GetComponent<Collider>();
            
            if (rb != null)
            {
                Vector3 distanceToTarget = targetPoint - firePoint.position;
                Vector3 gravity = Physics.gravity;
                Vector3 exactLaunchVelocity = (distanceToTarget / flightTime) - (gravity * flightTime / 2f);

                rb.linearVelocity = exactLaunchVelocity;

                Vector3 spinAxis = Vector3.Cross(Vector3.up, distanceToTarget.normalized);
                rb.angularVelocity = spinAxis * 10f; 
            }

            if (ballCollider != null)
            {
                StartCoroutine(EnableColliderAfterDelay(ballCollider, 0.2f));
            }
            
            Destroy(newBall, 3.5f);
        }
    }

    private IEnumerator EnableColliderAfterDelay(Collider col, float delay)
    {
        col.enabled = false;
        yield return new WaitForSeconds(delay);
        if (col != null) col.enabled = true;
    }
}