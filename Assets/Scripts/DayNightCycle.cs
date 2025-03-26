using UnityEngine;
using UnityEngine.Rendering;
using System;

public class DayNightCycle : MonoBehaviour
{
    public static DayNightCycle Instance { get; private set; }

    [Header("Time Settings")]
    [Tooltip("Duration of one full day/night cycle in seconds")]
    public float dayDuration = 300f; // 5 minutes per day
    [Tooltip("Current time of day (0-1)")]
    public float currentTime = 0.5f; // Start at noon
    [Tooltip("Start of dawn transition (0-1)")]
    public float dawnStart = 0.19f; // Adjusted from 0.25 to match -0.06 requirement
    [Tooltip("Start of dusk transition (0-1)")]
    public float duskStart = 0.5f;

    [Header("Day Counter")]
    [Tooltip("Current day number")]
    [SerializeField] private int _dayCount = 1; // Start on day 1
    public int DayCount => _dayCount; // Public getter for day count
    
    // Event that other scripts can subscribe to
    public event Action<int> OnDayChanged;

    [Header("Light Settings")]
    public Light mainLight;
    [Tooltip("Day light intensity")]
    public float dayLightIntensity = 1.5f; // Increased to make distant objects more visible
    [Tooltip("Night light intensity")]
    public float nightLightIntensity = 0.0f; // No light during night
    [Tooltip("Day light color")]
    public Color dayLightColor = new Color(1f, 0.95f, 0.8f); // Warm sunlight
    [Tooltip("Night light color")]
    public Color nightLightColor = new Color(0.6f, 0.6f, 1f); // Cool moonlight
    
    [Header("Sky Settings")]
    [Tooltip("Adjust ambient light color based on time of day")]
    public bool updateAmbientLight = true;
    [Tooltip("Day ambient light color")]
    public Color dayAmbientColor = new Color(0.6f, 0.6f, 0.6f); // Increased brightness
    [Tooltip("Night ambient light color")]
    public Color nightAmbientColor = new Color(0f, 0f, 0f); // Completely black
    [Tooltip("Ambient light mode")]
    public AmbientMode ambientMode = AmbientMode.Flat;
    [Tooltip("Day ambient intensity")]
    public float dayAmbientIntensity = 1.2f; // Increased to improve distant visibility
    [Tooltip("Day reflection intensity")]
    public float dayReflectionIntensity = 1.0f;

    private float _lastTimeCheck = 0f;
    private bool _dayChanged = false;

    private void Awake()
    {
        // Singleton pattern to allow easy access from other scripts
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (mainLight == null)
        {
            Debug.LogError("Please assign a main directional light in the inspector!");
            enabled = false;
            return;
        }

        // Set ambient mode
        RenderSettings.ambientMode = ambientMode;

        // Store the current time to check for day changes
        _lastTimeCheck = currentTime;

        UpdateLighting();
    }

    private void Update()
    {
        // Save the previous time to check for day changes
        _lastTimeCheck = currentTime;

        // Update time
        currentTime += Time.deltaTime / dayDuration;
        
        // Check for day change (when we cross midnight)
        if (currentTime >= 1f)
        {
            currentTime = 0f;
            _dayCount++;
            _dayChanged = true;
            
            // Notify subscribers that the day has changed
            OnDayChanged?.Invoke(_dayCount);
            
            Debug.Log($"Day changed to {_dayCount}");
        }

        // Update lighting
        UpdateLighting();
    }

    private void UpdateLighting()
    {
        // Calculate light rotation (0-360 degrees)
        float lightRotation = currentTime * 360f;

        // Update light rotation
        // Adjust angle to ensure light hits distant mountains
        mainLight.transform.rotation = Quaternion.Euler(lightRotation, 170f, 0f);
        
        // Transition duration in time units
        float dawnDuration = duskStart - dawnStart;
        float duskDuration = (1.0f - duskStart) + dawnStart; // Wraps around from dusk to dawn
        
        // Calculate light intensity based on time of day
        float lightIntensity;
        Color lightColor;
        Color ambientColor;
        float ambientIntensity;
        float reflectionIntensity;

        if (currentTime < dawnStart || currentTime > duskStart) // Night
        {
            // For the exact dawn start time (-0.06 converted to the 0-1 range)
            if (currentTime < dawnStart)
            {
                // Calculate how close we are to dawn
                float nightProgress = 1.0f - (currentTime / dawnStart);
                lightIntensity = nightLightIntensity;
                lightColor = nightLightColor;
                ambientColor = nightAmbientColor;
                ambientIntensity = 0f;
                reflectionIntensity = 0f;
            }
            else // After dusk
            {
                float nightProgress = (currentTime - duskStart) / (1.0f - duskStart);
                lightIntensity = nightLightIntensity;
                lightColor = nightLightColor;
                ambientColor = nightAmbientColor;
                ambientIntensity = 0f;
                reflectionIntensity = 0f;
            }
        }
        else if (currentTime < duskStart) // Dawn to Day to Dusk
        {
            // In the "daytime" range
            if (currentTime < (dawnStart + dawnDuration * 0.3f)) // Dawn transition
            {
                float transitionProgress = (currentTime - dawnStart) / (dawnDuration * 0.3f);
                lightIntensity = Mathf.Lerp(nightLightIntensity, dayLightIntensity, transitionProgress);
                lightColor = Color.Lerp(nightLightColor, dayLightColor, transitionProgress);
                ambientColor = Color.Lerp(nightAmbientColor, dayAmbientColor, transitionProgress);
                ambientIntensity = Mathf.Lerp(0f, dayAmbientIntensity, transitionProgress);
                reflectionIntensity = Mathf.Lerp(0f, dayReflectionIntensity, transitionProgress);
            }
            else if (currentTime < (duskStart - dawnDuration * 0.3f)) // Full day
            {
                lightIntensity = dayLightIntensity;
                lightColor = dayLightColor;
                ambientColor = dayAmbientColor;
                ambientIntensity = dayAmbientIntensity;
                reflectionIntensity = dayReflectionIntensity;
            }
            else // Dusk transition
            {
                float transitionProgress = (currentTime - (duskStart - dawnDuration * 0.3f)) / (dawnDuration * 0.3f);
                lightIntensity = Mathf.Lerp(dayLightIntensity, nightLightIntensity, transitionProgress);
                lightColor = Color.Lerp(dayLightColor, nightLightColor, transitionProgress);
                ambientColor = Color.Lerp(dayAmbientColor, nightAmbientColor, transitionProgress);
                ambientIntensity = Mathf.Lerp(dayAmbientIntensity, 0f, transitionProgress);
                reflectionIntensity = Mathf.Lerp(dayReflectionIntensity, 0f, transitionProgress);
            }
        }
        else // Should never happen but just in case
        {
            lightIntensity = dayLightIntensity;
            lightColor = dayLightColor;
            ambientColor = dayAmbientColor;
            ambientIntensity = dayAmbientIntensity;
            reflectionIntensity = dayReflectionIntensity;
        }

        // Apply light properties
        mainLight.intensity = lightIntensity;
        mainLight.color = lightColor;
        
        // Update ambient lighting
        if (updateAmbientLight)
        {
            RenderSettings.ambientLight = ambientColor;
            RenderSettings.ambientIntensity = ambientIntensity;
            RenderSettings.reflectionIntensity = reflectionIntensity;
        }

        // Only log on day change to reduce console spam
        if (_dayChanged)
        {
            Debug.Log($"Day: {_dayCount}, Time: {currentTime:F2}, Light: {lightIntensity:F2}");
            _dayChanged = false;
        }
    }

    // Public methods to access day and time info
    
    /// <summary>
    /// Get the current day count (starts at 1)
    /// </summary>
    public int GetCurrentDay()
    {
        return _dayCount;
    }

    /// <summary>
    /// Set the current day count
    /// </summary>
    public void SetDay(int day)
    {
        if (day < 1)
        {
            Debug.LogWarning("Day count cannot be less than 1. Setting to day 1.");
            day = 1;
        }
        
        if (_dayCount != day)
        {
            _dayCount = day;
            OnDayChanged?.Invoke(_dayCount);
        }
    }

    /// <summary>
    /// Returns true if it's currently daytime (between dawn and dusk)
    /// </summary>
    public bool IsDaytime()
    {
        return currentTime >= dawnStart && currentTime <= duskStart;
    }
    
    /// <summary>
    /// Returns true if it's currently nighttime
    /// </summary>
    public bool IsNighttime()
    {
        return !IsDaytime();
    }

    /// <summary>
    /// Skip to the next day
    /// </summary>
    public void SkipToNextDay()
    {
        _dayCount++;
        currentTime = dawnStart + 0.01f; // Just after dawn
        OnDayChanged?.Invoke(_dayCount);
        UpdateLighting();
    }
} 