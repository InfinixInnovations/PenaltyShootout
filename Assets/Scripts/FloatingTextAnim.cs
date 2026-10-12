using UnityEngine;

public class FloatingTextAnim : MonoBehaviour
{
    public float floatSpeed = 1f;
    public float fadeSpeed = 1f;

    void Update()
    {
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;
        transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
            Camera.main.transform.rotation * Vector3.up);
    }
}
