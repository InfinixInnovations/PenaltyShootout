using UnityEngine;

public class ResetBall : MonoBehaviour
{
    [Header("Ball Position")]
    [SerializeField] private GameObject ballPosition;
    private PenaltyKick penaltyKick;
    Vector3 originPos;

    private void Start() // IDK WHY I'M DOING THIS ON A DIFFERENT SCRIPT
    {
        originPos = ballPosition.transform.position;
        penaltyKick = ballPosition.GetComponent<PenaltyKick>();
    }
    public void ResetBallPos()
    {
        penaltyKick.ResetBall(originPos);
    }
}
