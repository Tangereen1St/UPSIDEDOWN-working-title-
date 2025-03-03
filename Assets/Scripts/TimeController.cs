using UnityEngine;
using UnityEngine.UI;

public class TimeController : MonoBehaviour
{
    [SerializeField] private DayNightCycle dayNightCycle;
    
    [Header("Time Control")]
    [SerializeField] private KeyCode speedUpKey = KeyCode.RightArrow;
    [SerializeField] private KeyCode slowDownKey = KeyCode.LeftArrow;
    [SerializeField] private KeyCode pauseKey = KeyCode.Space;
    [SerializeField] private KeyCode resetTimeKey = KeyCode.R;
    
    [Header("Time Scale")]
    [SerializeField] private float minTimeScale = 0.1f;
    [SerializeField] private float maxTimeScale = 100f;
    [SerializeField] private float timeScaleStep = 0.5f;
    [SerializeField] private float defaultTimeScale = 1f;
    
    [Header("UI Elements (Optional)")]
    [SerializeField] private Slider timeSlider;
    [SerializeField] private Text timeScaleText;
    
    private bool isPaused = false;
    private float currentTimeScale;
    
    private void Start()
    {
        if (dayNightCycle == null)
        {
            dayNightCycle = FindObjectOfType<DayNightCycle>();
            if (dayNightCycle == null)
            {
                Debug.LogError("TimeController: No DayNightCycle found in the scene!");
                enabled = false;
                return;
            }
        }
        
        // Initialize time scale
        currentTimeScale = defaultTimeScale;
        dayNightCycle.SetTimeScale(currentTimeScale);
        
        // Initialize UI elements if assigned
        if (timeSlider != null)
        {
            timeSlider.minValue = 0;
            timeSlider.maxValue = 24;
            timeSlider.value = dayNightCycle.GetTimeOfDay();
            timeSlider.onValueChanged.AddListener(OnTimeSliderChanged);
        }
        
        UpdateTimeScaleUI();
    }
    
    private void Update()
    {
        // Handle time slider update
        if (timeSlider != null)
        {
            timeSlider.value = dayNightCycle.GetTimeOfDay();
        }
        
        // Handle keyboard input
        HandleKeyboardInput();
    }
    
    private void HandleKeyboardInput()
    {
        // Pause/Resume time
        if (Input.GetKeyDown(pauseKey))
        {
            isPaused = !isPaused;
            dayNightCycle.FreezeTime(isPaused);
        }
        
        // Speed up time
        if (Input.GetKey(speedUpKey))
        {
            currentTimeScale = Mathf.Min(currentTimeScale + timeScaleStep * Time.deltaTime, maxTimeScale);
            dayNightCycle.SetTimeScale(currentTimeScale);
            UpdateTimeScaleUI();
        }
        
        // Slow down time
        if (Input.GetKey(slowDownKey))
        {
            currentTimeScale = Mathf.Max(currentTimeScale - timeScaleStep * Time.deltaTime, minTimeScale);
            dayNightCycle.SetTimeScale(currentTimeScale);
            UpdateTimeScaleUI();
        }
        
        // Reset time scale
        if (Input.GetKeyDown(resetTimeKey))
        {
            currentTimeScale = defaultTimeScale;
            dayNightCycle.SetTimeScale(currentTimeScale);
            UpdateTimeScaleUI();
        }
    }
    
    private void OnTimeSliderChanged(float value)
    {
        dayNightCycle.SetTimeOfDay(value);
    }
    
    private void UpdateTimeScaleUI()
    {
        if (timeScaleText != null)
        {
            timeScaleText.text = $"Time Scale: {currentTimeScale:F1}x";
        }
    }
    
    // Public methods for UI buttons
    public void SetTime(float time)
    {
        dayNightCycle.SetTimeOfDay(time);
    }
    
    public void SetTimeScale(float scale)
    {
        currentTimeScale = scale;
        dayNightCycle.SetTimeScale(scale);
        UpdateTimeScaleUI();
    }
    
    public void TogglePause()
    {
        isPaused = !isPaused;
        dayNightCycle.FreezeTime(isPaused);
    }
} 