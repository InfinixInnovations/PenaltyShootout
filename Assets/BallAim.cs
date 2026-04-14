using UnityEngine;
using UnityEngine.UI;

public class TargetSelector : MonoBehaviour
{
    [Header("Targets")]
    public Transform[] targets;
    private int currentTargetIndex = 2;

    [Header("Indicator")]
    public float moveSpeed = 10f;

    [Header("Power Settings")]
    public float maxPower = 100f;
    public float baseSpeed = 80f;
    public float fastSpeed = 140f;
    public float speedChangePoint = 70f;

    private float currentPower;
    private bool goingUp = true;

    [Header("UI")]
    public Slider powerBar;

    [Header("References")]
    public BallShooter ballShooter;

    [Header("Animation")]
    public Animator playerAnimator;

    private enum State { Aiming, Power, Waiting }
    private State currentState = State.Aiming;

    void Start()
    {
        powerBar.gameObject.SetActive(false);
    }

    void Update()
    {
        if (currentState == State.Aiming)
        {
            HandleInput();
            MoveIndicator();

            if (Input.GetKeyDown(KeyCode.Space))
            {
                LockTarget();
            }
        }
        else if (currentState == State.Power)
        {
            HandlePower();

            if (Input.GetKeyDown(KeyCode.Space))
            {
                LockPower();
            }
        }
    }

    void HandleInput()
    {
        int horizontal = 0;
        int vertical = 0;

        if (Input.GetKey(KeyCode.A)) horizontal = -1;
        if (Input.GetKey(KeyCode.D)) horizontal = 1;
        if (Input.GetKey(KeyCode.W)) vertical = 1;
        if (Input.GetKey(KeyCode.S)) vertical = -1;

        if (horizontal == -1 && vertical == 1)
            currentTargetIndex = 1;
        else if (horizontal == -1)
            currentTargetIndex = 0;
        else if (horizontal == 1 && vertical == 1)
            currentTargetIndex = 3;
        else if (horizontal == 1)
            currentTargetIndex = 4;
        else
            currentTargetIndex = 2;
    }

    void MoveIndicator()
    {
        Transform target = targets[currentTargetIndex];

        transform.position = Vector3.Lerp(
            transform.position,
            target.position,
            Time.deltaTime * moveSpeed
        );
    }

    void LockTarget()
    {
        currentState = State.Power;
        powerBar.gameObject.SetActive(true);

        currentPower = 0f;
        goingUp = true;
    }

    void HandlePower()
    {
        float speed = (currentPower >= speedChangePoint) ? fastSpeed : baseSpeed;

        if (goingUp)
        {
            currentPower += speed * Time.deltaTime;
            if (currentPower >= maxPower)
            {
                currentPower = maxPower;
                goingUp = false;
            }
        }
        else
        {
            currentPower -= speed * Time.deltaTime;
            if (currentPower <= 0)
            {
                currentPower = 0;
                goingUp = true;
            }
        }

        powerBar.value = currentPower;
    }

    void LockPower()
    {
        Transform selectedTarget = targets[currentTargetIndex];
        float selectedPower = currentPower;

        currentState = State.Waiting;

        // 🎬 Play animation
        playerAnimator.SetTrigger("Kick");

        // ❌ Hide UI + target indicator
        powerBar.gameObject.SetActive(false);
        gameObject.SetActive(false);

        // ⚽ Shoot
        ballShooter.Shoot(selectedTarget, selectedPower);
    }

    // 🔁 RESET SYSTEM
    public void ResetToAiming()
    {
        // ✅ Show UI + target again
        gameObject.SetActive(true);
        powerBar.gameObject.SetActive(true);

        currentState = State.Aiming;

        currentTargetIndex = 2;
        currentPower = 0f;

        powerBar.value = 0f;

        // 🔥 Reset animation cleanly
        playerAnimator.Rebind();
        playerAnimator.Update(0f);
    }
}