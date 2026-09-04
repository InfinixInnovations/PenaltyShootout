using UnityEngine;

public class ScoreTrigger : MonoBehaviour
{
    // This runs automatically whenever something touches the Trigger box
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that touched us is actually the soccer ball
        if (other.CompareTag("Ball"))
        {
            BallBehavior ball = other.GetComponent<BallBehavior>();
            if (ball != null)
            {
                ball.hasScored = true; // Mark as scored so OnDestroy doesn't trigger a save
            }

            // Minus the score!
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.DeductScore();
                ScoreManager.Instance.PlayGoalSound();
                ScoreManager.Instance.ShowFloatingText("GOAL!", other.transform.position, Color.red);
            }

            // Optional: Destroy the ball immediately so it doesn't bounce around 
            // and accidentally touch the trigger a second time!
            Destroy(other.gameObject);
        }
    }
}