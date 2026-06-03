using UnityEngine;

public class TrackSkeletonToRig : MonoBehaviour
{
    [System.Serializable]
    public class TargetPair
    {
        public Transform source;
        public Transform target;
        public Vector3 offset;
    }

    [Header("Hips")]
    [SerializeField] Transform hipsBone;
    [SerializeField] Transform hipLeftSource;
    [SerializeField] Transform hipRightSource;
    [SerializeField] Vector3 hipsOffset;

    [Header("IK Targets")]
    [SerializeField] TargetPair leftFoot;
    [SerializeField] TargetPair rightFoot;
    [SerializeField] TargetPair leftKnee;
    [SerializeField] TargetPair rightKnee;

    private void LateUpdate()
    {
        if (hipsBone != null && hipLeftSource != null && hipRightSource != null)
        {
            Vector3 hipCenter = (hipLeftSource.position + hipRightSource.position) * 0.5f;
            hipsBone.position = hipCenter + hipsOffset;
        }

        ApplyPair(leftFoot);
        ApplyPair(leftKnee);
        ApplyPair(rightFoot);
        ApplyPair(rightKnee);
    }

    private void ApplyPair (TargetPair p)
    {
        if (p == null && p.source == null && p.target == null) return;

        p.target.position = p.source.position + p.offset;
    }
}
