using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Header("Time Settings")]
    [Range(0, 24)]
    [SerializeField] private float timeOfDay = 12f; // Current time of day (24-hour format)
    [SerializeField] private float timeScale = 1f; // How fast time passes
    [SerializeField] private bool freezeTime = false; // If true, time won't progress

    [Header("Lighting")]
    [SerializeField] private Light directionalLight; // The sun
    [SerializeField] private float maxSunIntensity = 2f; // Maximum intensity of the sun
    [SerializeField] private float minSunIntensity = 0f; // Minimum intensity during night
    [SerializeField] private Color dayColor = Color.white; // Color of the sun during day
    [SerializeField] private Color sunriseColor = new Color(1f, 0.8f, 0.5f); // Color during sunrise
    [SerializeField] private Color sunsetColor = new Color(1f, 0.5f, 0f); // Color during sunset
    [SerializeField] private Color nightColor = Color.black; // Color during night

    [Header("Skybox")]
    [SerializeField] private Material daySkybox; // Skybox for day
    [SerializeField] private Material nightSkybox; // Skybox for night
    [SerializeField] private Material sunriseSkybox; // Skybox for sunrise
    [SerializeField] private Material sunsetSkybox; // Skybox for sunset
    private Material blendedSkybox; // Material used for transitions
    private static readonly int BlendAmount = Shader.PropertyToID("_BlendAmount");
    private static readonly int TransitionState = Shader.PropertyToID("_TransitionState");

    [Header("Ambient")]
    [SerializeField] private Color dayAmbient = new Color(0.5f, 0.5f, 0.5f);
    [SerializeField] private Color nightAmbient = new Color(0.0f, 0.0f, 0.0f);
    [SerializeField] private Color sunriseAmbient = new Color(0.4f, 0.4f, 0.4f);
    [SerializeField] private Color sunsetAmbient = new Color(0.3f, 0.3f, 0.3f);

    [Header("Night Settings")]
    [SerializeField] private bool pitchBlackNight = true;
    [SerializeField] private float moonlightIntensity = 0.05f;

    // Events that other scripts can subscribe to
    public delegate void TimeOfDayChanged(float timeOfDay);
    public static event TimeOfDayChanged OnTimeOfDayChanged;

    // Time constants
    private const float NIGHT_END = 5f;      // When night ends/sunrise starts
    private const float SUNRISE_END = 7f;    // When sunrise ends/day starts
    private const float SUNSET_START = 17f;  // When day ends/sunset starts
    private const float NIGHT_START = 19f;   // When sunset ends/night starts

    private void Start()
    {
        // If no directional light is assigned, try to find one
        if (directionalLight == null)
        {
            Light[] lights = FindObjectsOfType<Light>();
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    directionalLight = light;
                    break;
                }
            }
        }

        // Set initial lighting
        UpdateLighting();

        // Create a new material for blending if all skyboxes are assigned
        if (daySkybox != null && nightSkybox != null && sunriseSkybox != null && sunsetSkybox != null)
        {
            // Create a new blended skybox material
            blendedSkybox = new Material(Shader.Find("Custom/SkyboxBlend"));
            if (blendedSkybox != null)
            {
                // Get the main texture from each skybox
                Texture dayTex = daySkybox.GetTexture("_MainTex");
                Texture nightTex = nightSkybox.GetTexture("_MainTex");
                Texture sunriseTex = sunriseSkybox.GetTexture("_MainTex");
                Texture sunsetTex = sunsetSkybox.GetTexture("_MainTex");

                if (dayTex != null && nightTex != null && sunriseTex != null && sunsetTex != null)
                {
                    // Set the textures in our blend shader
                    blendedSkybox.SetTexture("_DayTex", dayTex);
                    blendedSkybox.SetTexture("_NightTex", nightTex);
                    blendedSkybox.SetTexture("_SunriseTex", sunriseTex);
                    blendedSkybox.SetTexture("_SunsetTex", sunsetTex);

                    // Copy exposure values if they exist
                    if (daySkybox.HasProperty("_Exposure"))
                        blendedSkybox.SetFloat("_DayExposure", daySkybox.GetFloat("_Exposure"));
                    if (nightSkybox.HasProperty("_Exposure"))
                        blendedSkybox.SetFloat("_NightExposure", nightSkybox.GetFloat("_Exposure"));
                    if (sunriseSkybox.HasProperty("_Exposure"))
                        blendedSkybox.SetFloat("_SunriseExposure", sunriseSkybox.GetFloat("_Exposure"));
                    if (sunsetSkybox.HasProperty("_Exposure"))
                        blendedSkybox.SetFloat("_SunsetExposure", sunsetSkybox.GetFloat("_Exposure"));

                    // Set initial blend values
                    blendedSkybox.SetFloat(BlendAmount, 0);
                    blendedSkybox.SetFloat(TransitionState, 0);
                    RenderSettings.skybox = blendedSkybox;
                }
                else
                {
                    Debug.LogError("Could not find main texture in one or more skybox materials. Make sure they are properly set up.");
                }
            }
            else
            {
                Debug.LogError("Failed to find Custom/SkyboxBlend shader. Make sure it's included in your project.");
            }
        }
        else
        {
            Debug.LogWarning("One or more skybox materials not assigned. Skybox blending will be disabled.");
        }
    }

    private void Update()
    {
        if (!freezeTime)
        {
            // Update time of day
            timeOfDay += Time.deltaTime * timeScale / 60f; // Divide by 60 to convert to hours
            
            // Wrap time of day between 0 and 24
            if (timeOfDay >= 24f)
            {
                timeOfDay -= 24f;
            }
        }

        // Update lighting based on time of day
        UpdateLighting();

        // Trigger event for other scripts
        OnTimeOfDayChanged?.Invoke(timeOfDay);
    }

    private void UpdateLighting()
    {
        if (directionalLight == null) return;

        // Calculate sun rotation
        float sunRotation = (timeOfDay / 24f) * 360f;
        directionalLight.transform.rotation = Quaternion.Euler(sunRotation - 90f, 170f, 0);

        // Calculate which transition period we're in and the blend amount
        float transitionState;
        float blendAmount;
        Color targetAmbient;
        float intensity;
        Color lightColor;

        if (timeOfDay < NIGHT_END) // Night
        {
            transitionState = 0;
            blendAmount = timeOfDay / NIGHT_END;
            intensity = pitchBlackNight ? 0f : moonlightIntensity;
            lightColor = nightColor;
            targetAmbient = nightAmbient;
        }
        else if (timeOfDay < SUNRISE_END) // Sunrise
        {
            transitionState = 1;
            blendAmount = (timeOfDay - NIGHT_END) / (SUNRISE_END - NIGHT_END);
            intensity = Mathf.Lerp(moonlightIntensity, maxSunIntensity, blendAmount);
            lightColor = Color.Lerp(sunriseColor, dayColor, blendAmount);
            targetAmbient = Color.Lerp(sunriseAmbient, dayAmbient, blendAmount);
        }
        else if (timeOfDay < SUNSET_START) // Day
        {
            transitionState = 2;
            blendAmount = (timeOfDay - SUNRISE_END) / (SUNSET_START - SUNRISE_END);
            intensity = maxSunIntensity;
            lightColor = dayColor;
            targetAmbient = dayAmbient;
        }
        else if (timeOfDay < NIGHT_START) // Sunset
        {
            transitionState = 3;
            blendAmount = (timeOfDay - SUNSET_START) / (NIGHT_START - SUNSET_START);
            intensity = Mathf.Lerp(maxSunIntensity, moonlightIntensity, blendAmount);
            lightColor = Color.Lerp(dayColor, sunsetColor, blendAmount);
            targetAmbient = Color.Lerp(dayAmbient, sunsetAmbient, blendAmount);
        }
        else // Night
        {
            transitionState = 0;
            blendAmount = 0;
            intensity = pitchBlackNight ? 0f : moonlightIntensity;
            lightColor = nightColor;
            targetAmbient = nightAmbient;
        }

        // Apply calculated values
        directionalLight.intensity = intensity;
        directionalLight.color = lightColor;
        RenderSettings.ambientLight = targetAmbient;

        // Update skybox if available
        if (blendedSkybox != null)
        {
            blendedSkybox.SetFloat(TransitionState, transitionState);
            blendedSkybox.SetFloat(BlendAmount, blendAmount);
        }
    }

    // Public methods to control time
    public void SetTimeOfDay(float newTime)
    {
        timeOfDay = Mathf.Clamp(newTime, 0f, 24f);
        UpdateLighting();
    }

    public float GetTimeOfDay()
    {
        return timeOfDay;
    }

    public void SetTimeScale(float scale)
    {
        timeScale = scale;
    }

    public void FreezeTime(bool freeze)
    {
        freezeTime = freeze;
    }
    
    public void SetPitchBlackNight(bool enabled)
    {
        pitchBlackNight = enabled;
        UpdateLighting();
    }
} 