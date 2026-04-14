using UnityEngine;

public class GoalTarget : MonoBehaviour
{
    public int points = 50;

    private void OnTriggerEnter(Collider other)
    {
        // 🔍 Debug: anything entering
        Debug.Log("Trigger entered by: " + other.name);

        // 🔍 Debug: check tag
        Debug.Log("Tag of object: " + other.tag);

        if (other.CompareTag("Ball"))
        {
            Debug.Log("✅ Ball hit target: " + gameObject.name);

            // Change color for visual confirmation
            Renderer rend = GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.color = Color.red;
            }

            // Destroy after slight delay (so you can see it)
            Destroy(gameObject, 0.002f);
        }
    }
}