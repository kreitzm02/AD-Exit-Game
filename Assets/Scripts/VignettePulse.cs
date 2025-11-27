using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VignettePulse : MonoBehaviour
{
    [Header("VIGNETTE BASE")]
    [SerializeField] private Volume volume;
    [SerializeField] private float baseIntensity = 0.18f;

    [Header("PULSE")]
    [SerializeField] private bool enablePulse = true;
    [SerializeField] private float pulseStrength = 0.04f;
    [SerializeField] private float pulseSpeed = 0.6f;

    private Vignette vignette;
    private float noiseTime;

    private void Awake()
    {
        if (!volume)
            volume = FindFirstObjectByType<Volume>();

        volume.profile.TryGet(out vignette);
    }

    private void Update()
    {
        if (!enablePulse || vignette == null)
            return;

        noiseTime += Time.deltaTime * pulseSpeed;

        float noise = Mathf.PerlinNoise(noiseTime, 0f);
        float pulseOffset = (noise - 0.5f) * pulseStrength;

        vignette.intensity.value = baseIntensity + pulseOffset;
    }

    public void SetPulseStrength(float strength)
    {
        pulseStrength = strength;
    }

    public void SetBaseIntensity(float intensity)
    {
        baseIntensity = intensity;
    }
}
