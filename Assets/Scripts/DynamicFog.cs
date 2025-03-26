using UnityEngine;

public class DynamicFog : MonoBehaviour
{
    [Header("Basic Fog Settings")]
    [Tooltip("Day fog density")]
    public float dayFogDensity = 0.01f;
    [Tooltip("Night fog density")]
    public float nightFogDensity = 0.05f;
    [Tooltip("Day fog color")]
    public Color dayFogColor = new Color(0.75f, 0.75f, 0.85f);
    [Tooltip("Night fog color")]
    public Color nightFogColor = new Color(0.1f, 0.1f, 0.2f);
    [Tooltip("How quickly fog transitions between day and night")]
    public float transitionSpeed = 2.0f;
    
    // Reference to day/night cycle
    private DayNightCycle dayNightCycle;
    
    // Current fog values
    private float currentDensity;
    private Color currentColor;
    
    private void Start()
    {
        // Ensure fog is enabled
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        
        // Get reference to day/night cycle
        dayNightCycle = FindObjectOfType<DayNightCycle>();
        if (dayNightCycle == null)
        {
            Debug.LogWarning("DynamicFog: DayNightCycle not found. Using default day fog settings.");
            ApplyFogSettings(dayFogDensity, dayFogColor);
        }
        else
        {
            // Apply initial fog based on time of day
            UpdateFog();
        }
    }
    
    private void Update()
    {
        if (dayNightCycle != null)
        {
            UpdateFog();
        }
    }
    
    private void UpdateFog()
    {
        // Get target fog values based on time of day
        float targetDensity;
        Color targetColor;
        
        if (dayNightCycle.IsDaytime())
        {
            targetDensity = dayFogDensity;
            targetColor = dayFogColor;
        }
        else
        {
            targetDensity = nightFogDensity;
            targetColor = nightFogColor;
        }
        
        // Smoothly transition
        currentDensity = Mathf.Lerp(currentDensity, targetDensity, Time.deltaTime * transitionSpeed);
        currentColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * transitionSpeed);
        
        // Apply the fog settings
        ApplyFogSettings(currentDensity, currentColor);
    }
    
    private void ApplyFogSettings(float density, Color color)
    {
        RenderSettings.fogDensity = density;
        RenderSettings.fogColor = color;
    }
    
    // Public method to manually set fog density
    public void SetFogDensity(float density)
    {
        dayFogDensity = density;
        nightFogDensity = density;
        ApplyFogSettings(density, RenderSettings.fogColor);
    }
    
    // Public method to manually set fog color
    public void SetFogColor(Color color)
    {
        dayFogColor = color;
        nightFogColor = color;
        ApplyFogSettings(RenderSettings.fogDensity, color);
    }
} 