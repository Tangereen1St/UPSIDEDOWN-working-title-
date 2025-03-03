# Day-Night Cycle System

This package provides a complete day-night cycle system for Unity games. It simulates the passage of time with realistic lighting changes, including sunrise, day, sunset, and night phases.

## Features

- Realistic sun movement and lighting changes
- Smooth transitions between day and night
- Configurable time scale (speed up or slow down time)
- Optional skybox transitions
- Ambient lighting changes
- UI time display
- Time control system (pause, speed up, slow down)

## Setup Instructions

1. **Add the DayNightCycle component to a GameObject in your scene:**
   - Create an empty GameObject and name it "DayNightCycle"
   - Add the `DayNightCycle.cs` script to this GameObject

2. **Configure the DayNightCycle component:**
   - Assign a directional light to the "Directional Light" field (or let it auto-detect one)
   - Adjust the time settings, lighting colors, and intensities as needed
   - Optionally assign day and night skybox materials

3. **Add the TimeDisplay component (optional):**
   - Create a UI Text element in your canvas
   - Add the `TimeDisplay.cs` script to this GameObject
   - Assign the TextMeshProUGUI component to the "Time Text" field
   - Assign the DayNightCycle component to the "Day Night Cycle" field

4. **Add the TimeController component (optional):**
   - Add the `TimeController.cs` script to any GameObject (can be the same as DayNightCycle)
   - Assign the DayNightCycle component to the "Day Night Cycle" field
   - Configure the keyboard controls and time scale settings
   - Optionally set up UI elements for time control

## Usage

### Basic Usage

The system will automatically update the lighting based on the time of day. The default time scale is 1, which means 1 minute of real time equals 1 hour of game time.

### Controlling Time Programmatically

You can control the time of day and time scale programmatically using the following methods:

```csharp
// Get a reference to the DayNightCycle component
DayNightCycle dayNightCycle = FindObjectOfType<DayNightCycle>();

// Set the time of day (0-24)
dayNightCycle.SetTimeOfDay(12f); // Set to noon

// Get the current time of day
float currentTime = dayNightCycle.GetTimeOfDay();

// Set the time scale (how fast time passes)
dayNightCycle.SetTimeScale(2f); // 2x speed

// Freeze/unfreeze time
dayNightCycle.FreezeTime(true); // Freeze time
dayNightCycle.FreezeTime(false); // Unfreeze time
```

### Keyboard Controls (with TimeController)

- **Right Arrow**: Speed up time
- **Left Arrow**: Slow down time
- **Space**: Pause/resume time
- **R**: Reset time scale to default

### Events

You can subscribe to the `OnTimeOfDayChanged` event to be notified when the time of day changes:

```csharp
void Start()
{
    DayNightCycle.OnTimeOfDayChanged += HandleTimeChanged;
}

void OnDestroy()
{
    DayNightCycle.OnTimeOfDayChanged -= HandleTimeChanged;
}

void HandleTimeChanged(float timeOfDay)
{
    // Do something with the new time of day
    Debug.Log($"Time changed to {timeOfDay}");
}
```

## Customization

### Lighting Colors

You can customize the colors for different times of day:
- **Day Color**: The color of the sun during the day
- **Sunset Color**: The color during sunset
- **Night Color**: The color during night

### Time Constants

You can modify the time constants in the `DayNightCycle.cs` script to change when different phases of the day occur:

```csharp
private const float SUNRISE_START = 5f;
private const float SUNRISE_END = 7f;
private const float SUNSET_START = 17f;
private const float SUNSET_END = 19f;
private const float DAY_START = 7f;
private const float DAY_END = 17f;
private const float NIGHT_START = 19f;
private const float NIGHT_END = 5f;
```

## Troubleshooting

- **No directional light found**: Make sure you have a directional light in your scene or assign one manually.
- **Lighting not changing**: Check if the time is frozen or if the time scale is set to 0.
- **UI not updating**: Make sure the TextMeshProUGUI component is properly assigned to the TimeDisplay script.

## Advanced Customization

For more advanced customization, you can modify the `UpdateLighting()` method in the `DayNightCycle.cs` script to change how lighting is calculated based on the time of day. 