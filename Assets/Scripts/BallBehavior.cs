using UnityEngine;
using TMPro;

public class BallBehavior : MonoBehaviour
{
    public bool hasScored = false;
    private bool isShuttingDown = false;
    void Start()
    {
    }

    void OnApplicationQuit() { isShuttingDown = true; }

    private void OnCollisionEnter(Collision collision)
    {
        // Removed bounce sound
    }

    private void OnDestroy()
    {
        if (isShuttingDown) return;
        if (!hasScored && gameObject.scene.isLoaded)
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.PlaySaveSound();
                ScoreManager.Instance.ShowFloatingText("SAVE!", transform.position, Color.green);
            }
        }
    }
}
