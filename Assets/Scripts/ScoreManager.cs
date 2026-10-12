using System.Collections;
using UnityEngine;
using TMPro; 
using UnityEngine.SceneManagement;
using UnityEngine.XR;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("UI Setup")]
    public TextMeshProUGUI scoreText; 
    public TextMeshProUGUI timerText; 

    [Header("Game Settings")]
    [Tooltip("Seconds before the game actually begins")]
    public float startDelay = 3f; 
    [Tooltip("How many seconds the game lasts")]
    public float gameDuration = 60f; 

    [Header("Audio & Effects")]
    public AudioClip saveClip;
    public AudioClip goalClip;
    public AudioClip bounceClip;
    public GameObject floatingTextPrefab;
    private AudioSource audioSource;
    
    private int currentScore = 0;
    private bool isGameStarted = false;
    private bool isGameOver = false;
    private float restartHoldTimer = 0f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void Start()
    {
        // 1. Hide the score text completely so the player focuses on the game!
        if (scoreText != null) 
        {
            scoreText.gameObject.SetActive(false);
        }

        // 2. Start the dramatic countdown
        StartCoroutine(GameStartSequence());
    }

    IEnumerator GameStartSequence()
    {
        // The "Get Ready" Phase
        float countdown = startDelay;
        while (countdown > 0)
        {
            if (timerText != null) timerText.text = "Get Ready: " + Mathf.CeilToInt(countdown).ToString();
            yield return new WaitForSeconds(1f);
            countdown--;
        }

        // The Action Phase!
        isGameStarted = true;
        if (timerText != null) timerText.text = "GO!";

        // Find the CannonManager and tell it to start firing
        CannonManager cannonManager = FindFirstObjectByType<CannonManager>();
        if (cannonManager != null)
        {
            cannonManager.StartCannons();
        }
    }

    void Update()
    {
        if (isGameOver)
        {
            // Listen for 'B' button (secondary button on right controller)
            InputDevice rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            if (rightHand.TryGetFeatureValue(CommonUsages.secondaryButton, out bool bPressed) && bPressed)
            {
                restartHoldTimer += Time.deltaTime;
                if (restartHoldTimer >= 1.5f)
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }
            }
            else
            {
                restartHoldTimer = 0f;
            }
            return;
        }

        // Don't run the main clock if we are in the intro delay
        if (!isGameStarted) return;

        gameDuration -= Time.deltaTime;

        if (timerText != null)
        {
            timerText.text = "Time: " + Mathf.CeilToInt(gameDuration).ToString();
        }

        if (gameDuration <= 0)
        {
            TriggerGameOver();
        }
    }

    public void AddShot()
    {
        if (!isGameStarted || isGameOver) return;
        currentScore++;
        // We do NOT update the UI here anymore because it is hidden!
    }

    public void DeductScore()
    {
        if (!isGameStarted || isGameOver) return;
        currentScore--;
    }

    private void TriggerGameOver()
    {
        isGameOver = true;

        // 1. Tell the cannons to shut down
        CannonManager cannonManager = FindFirstObjectByType<CannonManager>();
        if (cannonManager != null)
        {
            cannonManager.StopCannons();
        }

        // 2. Flash Game Over, Final Score, and instructions on the timer
        if (timerText != null) 
        {
            timerText.text = "GAME OVER!\nFinal Score: " + currentScore.ToString() + "\nHold 'B' to Restart";
        }

        // 3. Keep scoreText hidden to prevent overlapping text on the screen
        if (scoreText != null) 
        {
            scoreText.gameObject.SetActive(false);
        }
    }

    public void PlaySaveSound()
    {
        if (audioSource != null && saveClip != null)
        {
            audioSource.PlayOneShot(saveClip);
        }
    }

    public void PlayGoalSound()
    {
        if (audioSource != null && goalClip != null)
        {
            audioSource.PlayOneShot(goalClip);
        }
    }

    public void ShowFloatingText(string text, Vector3 position, Color color)
    {
        if (floatingTextPrefab != null)
        {
            // Position slightly above the object
            GameObject ft = Instantiate(floatingTextPrefab, position + Vector3.up * 1.5f, Quaternion.identity);
            TextMeshPro tmpro = ft.GetComponentInChildren<TextMeshPro>();
            if (tmpro != null)
            {
                tmpro.text = text;
                tmpro.color = color;
            }
            
            // Add a simple float up script dynamically
            FloatingTextAnim anim = ft.AddComponent<FloatingTextAnim>();
            Destroy(ft, 1.5f);
        }
    }
}