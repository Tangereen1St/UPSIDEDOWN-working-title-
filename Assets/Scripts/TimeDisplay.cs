using UnityEngine;
using TMPro;
using System;

public class TimeDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private DayNightCycle dayNightCycle;

    private void Start()
    {
        if (dayNightCycle == null)
        {
            dayNightCycle = FindObjectOfType<DayNightCycle>();
        }

        if (timeText == null)
        {
            Debug.LogWarning("TimeDisplay: No TextMeshProUGUI component assigned!");
        }
    }

    private void Update()
    {
        if (dayNightCycle != null && timeText != null)
        {
            float timeOfDay = dayNightCycle.GetTimeOfDay();
            
            // Convert to hours and minutes
            int hours = Mathf.FloorToInt(timeOfDay);
            int minutes = Mathf.FloorToInt((timeOfDay - hours) * 60);
            
            // Format time as HH:MM
            timeText.text = $"{hours:00}:{minutes:00}";
            
            // Optionally add AM/PM indicator
            // string amPm = hours < 12 ? "AM" : "PM";
            // int displayHours = hours % 12;
            // if (displayHours == 0) displayHours = 12;
            // timeText.text = $"{displayHours}:{minutes:00} {amPm}";
        }
    }
} 