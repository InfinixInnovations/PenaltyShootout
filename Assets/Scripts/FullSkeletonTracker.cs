using UnityEngine;
using System.Collections.Generic;
using OscJack;

public class FullSkeletonTracker : MonoBehaviour
{
    private OscServer server;

    [System.Serializable]
    public class JointConfig
    {
        [Tooltip("The exact name of the joint in TouchDesigner (e.g., hand_l, head, knee_r)")]
        public string tdJointName;

        [Tooltip("The Unity GameObject/Sphere that represents this joint")]
        public Transform markerTransform;

        [HideInInspector] public Vector3 position;
        [HideInInspector] public bool hasNewData;
    }

    [Header("Skeleton Configuration")]
    public List<JointConfig> skeletonJoints = new List<JointConfig>();

    [Header("Presence Tracking Status")]
    [Tooltip("Read-only status indicating if a player is actively streaming live skeleton data")]
    public bool isTracked = false;

    [Tooltip("How long (in seconds) to wait before deciding the player left the frame")]
    public float networkTimeoutThreshold = 0.5f;

    private float timeSinceLastDataReceived = 0f;
    private readonly object dataLock = new object();

    void Start()
    {
        // Start the server on port 9000
        server = new OscServer(9000);

        // Dynamically loop through every joint added to the inspector list
        foreach (var joint in skeletonJoints)
        {
            var currentJoint = joint;

            // Create network listeners for X, Y, and Z addresses automatically
            server.MessageDispatcher.AddCallback($"/p1/{currentJoint.tdJointName}:tx", (string addr, OscDataHandle data) => {
                lock (dataLock)
                {
                    currentJoint.position.x = data.GetElementAsFloat(0);
                    currentJoint.hasNewData = true;
                    timeSinceLastDataReceived = 0f; // Reset timeout counter on data packet arrival
                }
            });

            server.MessageDispatcher.AddCallback($"/p1/{currentJoint.tdJointName}:ty", (string addr, OscDataHandle data) => {
                lock (dataLock)
                {
                    currentJoint.position.y = data.GetElementAsFloat(0);
                    currentJoint.hasNewData = true;
                    timeSinceLastDataReceived = 0f;
                }
            });

            server.MessageDispatcher.AddCallback($"/p1/{currentJoint.tdJointName}:tz", (string addr, OscDataHandle data) => {
                lock (dataLock)
                {
                    currentJoint.position.z = data.GetElementAsFloat(0);
                    currentJoint.hasNewData = true;
                    timeSinceLastDataReceived = 0f;
                }
            });
        }
    }

    void Update()
    {
        // Track passing time since the background threads last touched the timer
        timeSinceLastDataReceived += Time.deltaTime;

        // If data hasn't arrived within our window limit, mark the user as untracked
        if (timeSinceLastDataReceived >= networkTimeoutThreshold)
        {
            isTracked = false;
        }

        // Thread-safely update all tracked transforms on Unity's main frame loop
        lock (dataLock)
        {
            foreach (var joint in skeletonJoints)
            {
                if (joint.hasNewData)
                {
                    // If even a single joint is getting fresh packets, the player is active!
                    isTracked = true;

                    if (joint.markerTransform != null)
                    {
                        joint.markerTransform.localPosition = joint.position;
                    }
                    joint.hasNewData = false;
                }
            }
        }
    }

    void OnDestroy()
    {
        if (server != null)
        {
            server.Dispose();
            server = null;
        }
    }
}