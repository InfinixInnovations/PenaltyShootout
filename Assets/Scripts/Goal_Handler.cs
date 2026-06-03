using System.Collections;
using TMPro;
using UnityEngine;

public class Goal_Handler : MonoBehaviour
{
    [Header("Ball")]
    [SerializeField] private GameObject ball;

    [Header("UI")]
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private string goalMessage = "GOALL!!";
    [SerializeField] private string overshotMessage = "Overshot :(";
    [SerializeField] private string undershotMessage = "Undershot :(";
    [SerializeField] private Color successColour = new Color(34, 189, 42);
    [SerializeField] private Color failureColour = new Color(235, 64, 52);

    [Header("Timing")]
    [SerializeField] private float judgeDelay = 0.8f;
    [SerializeField] private float messageDuration = 3f;


    private bool shotInProgress;
    private bool goalScored;

    private void Start()
    {
        SetText("", Color.white);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject != ball) return;
        if (!shotInProgress) return;

        goalScored = true;
        SetText(goalMessage, successColour);
    }

    private void SetText(string message, Color colour)
    {
        resultText.text = message;
        resultText.color = colour;
    }

    IEnumerator JudgeShot()
    {
        yield return new WaitForSeconds(judgeDelay);
        if (!goalScored)
            SetText(overshotMessage, failureColour);

        yield return new WaitForSeconds(messageDuration);
        SetText("", Color.white);
        shotInProgress = false;
    }

    public void RegisterShot()
    {
        if (shotInProgress) return;
        shotInProgress = true;
        goalScored = false;
        StartCoroutine(JudgeShot());
    }
}
