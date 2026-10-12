using UnityEngine;

public class StadiumLightManager : MonoBehaviour
{
    [Header("Stadium Lights")]
    public Light[] roofLights;
    public Material lightBulbMaterial;

    [Header("Light Settings")]
    public Color lightColor = new Color(0.9f, 0.95f, 1f);
    public float intensity = 50f;
    public float range = 150f;
    public float spotAngle = 80f;
    public float innerSpotAngle = 50f;
    
    [Header("Material Settings")]
    [Tooltip("How bright the physical bulb meshes glow")]
    public float bulbGlowIntensity = 50f;

    private void OnValidate()
    {
        if (roofLights != null)
        {
            foreach (var l in roofLights)
            {
                if (l != null)
                {
                    l.color = lightColor;
                    l.intensity = intensity;
                    l.range = range;
                    l.spotAngle = spotAngle;
                    l.innerSpotAngle = innerSpotAngle;
                }
            }
        }

        if (lightBulbMaterial != null)
        {
            lightBulbMaterial.SetColor("_EmissionColor", lightColor * bulbGlowIntensity);
        }
    }
}
