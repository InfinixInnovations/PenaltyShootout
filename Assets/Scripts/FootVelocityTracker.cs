using UnityEngine;

public class FootVelocityTracker : MonoBehaviour
{
    Vector3 lastPos;
    public Vector3 Velocity { get; private set; }

    void Start() => lastPos = transform.position;

    void FixedUpdate()
    {
        Velocity = (transform.position - lastPos) / Time.fixedDeltaTime;
        lastPos = transform.position;
    }
}