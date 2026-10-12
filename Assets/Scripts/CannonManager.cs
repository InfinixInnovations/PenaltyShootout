using System.Collections;
using UnityEngine;

[System.Serializable]
public class CannonTargetPair
{
    public GoalieCannon cannon;
    public Transform specificTarget; 
}

public class CannonManager : MonoBehaviour
{
    [Header("Cannons & Targets Setup")]
    public CannonTargetPair[] cannonSetups;

    [Header("Targeting Randomness")]
    public float targetWidth = 2.2f; 
    public float targetHeight = 2.2f; 

    [Header("Physics Shot Master Settings")]
    public float flightTime = 1.0f; 

    [Header("Timing")]
    public float timeBetweenShots = 4f; 
    public float warningDuration = 1.0f; 

    // NEW: The ScoreManager triggers this exact function when the "Get Ready" timer hits 0
    public void StartCannons()
    {
        // 0f means it fires the very first shot instantly when told to go!
        InvokeRepeating("PickCannonAndShoot", 0f, timeBetweenShots);
    }

    void PickCannonAndShoot()
    {
        if (cannonSetups.Length == 0) return;

        CannonTargetPair selectedSetup = cannonSetups[Random.Range(0, cannonSetups.Length)];
        if (selectedSetup.cannon == null || selectedSetup.specificTarget == null) return;

        float randomX = Random.Range(-targetWidth / 2f, targetWidth / 2f);
        float randomY = Random.Range(-targetHeight / 2f, targetHeight / 2f);

        Vector3 finalTarget = selectedSetup.specificTarget.position 
                              + (selectedSetup.specificTarget.right * randomX) 
                              + (selectedSetup.specificTarget.up * randomY);

        StartCoroutine(TelegraphAndShoot(selectedSetup, finalTarget));
    }

    private IEnumerator TelegraphAndShoot(CannonTargetPair setup, Vector3 target)
    {
        setup.cannon.ToggleHighlight(true);
        yield return new WaitForSeconds(warningDuration);
        setup.cannon.ToggleHighlight(false);
        setup.cannon.AimAndShoot(target, flightTime);
    }

    public void StopCannons()
    {
        CancelInvoke("PickCannonAndShoot"); 
        StopAllCoroutines(); 
        
        foreach (CannonTargetPair setup in cannonSetups)
        {
            if (setup.cannon != null)
            {
                setup.cannon.ToggleHighlight(false);
            }
        }
    }
}