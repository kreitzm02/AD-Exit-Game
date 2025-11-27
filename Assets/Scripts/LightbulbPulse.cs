using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightbulbPulse : MonoBehaviour
{
    [Header("LIGHT")]
    [SerializeField] private Light2D light2D;

    [Header("BASE SETTINGS")]
    [SerializeField] private float baseIntensity = 1.0f;

    [Header("PULSE")]
    [SerializeField] private bool enablePulse = true;
    [SerializeField] private float pulseStrength = 0.25f;
    [SerializeField] private float pulseSpeed = 0.5f;

    private float noiseTime;

    private void Awake()
    {
        if (!light2D)
            light2D = GetComponent<Light2D>();
    }

    private void Update()
    {
        if (!enablePulse || light2D == null)
            return;

        noiseTime += Time.deltaTime * pulseSpeed;

        float noise = Mathf.PerlinNoise(noiseTime, 0f);
        float pulseOffset = (noise - 0.5f) * pulseStrength;

        light2D.intensity = baseIntensity + pulseOffset;
    }
}
