using UnityEngine;
using System.Collections.Generic;

public class security_cam_system : MonoBehaviour, IInteractable
{
    public List<GameObject> cameras;
    public int cameraSelected;
    public security_cam_system other_button;
    [Tooltip("Optional: Animator for button press animation")]
    public Animator buttonAnim;
    [Tooltip("Name of the animation clip to play when button is pressed")]
    public string buttonPressAnimationName = "buttonpress";
    
    [Header("Power Settings")]
    public bool isPowered = true;
    [Tooltip("Optional: The monitor/screen object to disable when powered off")]
    public GameObject monitorScreen;
    [Tooltip("The material to use when the monitor is powered off")]
    public Material monitorOffMaterial;
    [Tooltip("The RenderTexture to display when powered on")]
    public RenderTexture monitorRenderTexture;
    
    [Header("Battery Settings")]
    [Tooltip("Maximum battery level")]
    public float maxBattery = 100f;
    [Tooltip("Current battery level")]
    public float currentBattery;
    [Tooltip("How fast the battery drains when system is powered on (units per second)")]
    public float batteryDrainRate = 1f;
    [Tooltip("Minimum battery required to power on")]
    public float minimumBatteryToOperate = 5f;

    private MeshRenderer monitorRenderer;
    private Material originalMaterial;

    void Start()
    {
        // Initialize battery
        currentBattery = maxBattery;
        
        // Get the monitor's renderer if it exists
        if (monitorScreen != null)
        {
            monitorRenderer = monitorScreen.GetComponent<MeshRenderer>();
            if (monitorRenderer == null)
            {
                Debug.LogError("Monitor screen object needs a MeshRenderer component!");
            }
            else
            {
                // Store the original material that has the RenderTexture
                originalMaterial = monitorRenderer.material;
            }
        }

        // Add validation
        Debug.Log($"Security cam system starting. Camera count: {cameras?.Count ?? 0}");
        
        // Initialize system with first camera active
        if (cameras != null && cameras.Count > 0)
        {
            UpdateCameraStates();
            Debug.Log("First camera activated");
        }
        else
        {
            Debug.LogError("No cameras assigned to security camera system!");
        }

        // Validate animation setup
        if (buttonAnim != null)
        {
            // Check if the animation exists
            AnimationClip[] clips = buttonAnim.runtimeAnimatorController.animationClips;
            bool foundAnim = false;
            foreach (AnimationClip clip in clips)
            {
                if (clip.name == buttonPressAnimationName)
                {
                    foundAnim = true;
                    break;
                }
            }
            if (!foundAnim)
            {
                Debug.LogWarning($"Animation clip '{buttonPressAnimationName}' not found in the Animator!");
            }
        }
    }

    void Update()
    {
        // Drain battery when powered on
        if (isPowered && currentBattery > 0)
        {
            currentBattery = Mathf.Max(0, currentBattery - (batteryDrainRate * Time.deltaTime));
            
            // Auto shutdown when battery is too low
            if (currentBattery < minimumBatteryToOperate)
            {
                Debug.Log("Battery too low - auto shutdown");
                ForcePowerOff();
            }
        }
    }

    public void TogglePower()
    {
        // Check if we can power on
        if (!isPowered && currentBattery < minimumBatteryToOperate)
        {
            Debug.Log("Not enough battery to power on!");
            return;
        }

        Debug.Log($"Toggling power from {isPowered} to {!isPowered}");
        isPowered = !isPowered;
        
        // Update camera states first
        UpdateCameraStates();
        
        // Then update monitor appearance
        if (monitorRenderer != null)
        {
            Debug.Log($"Monitor renderer found on {monitorScreen.name}");
            if (monitorOffMaterial != null && originalMaterial != null)
            {
                Debug.Log($"Switching material to {(isPowered ? "ON" : "OFF")} material");
                monitorRenderer.material = isPowered ? originalMaterial : monitorOffMaterial;
            }
            else
            {
                Debug.LogError("Materials not set up correctly! Original Material: " + 
                    (originalMaterial != null ? "Found" : "Missing") + 
                    ", Off Material: " + 
                    (monitorOffMaterial != null ? "Found" : "Missing"));
            }
        }
        else
        {
            Debug.LogError("No MeshRenderer found on monitor screen object!");
        }
        
        Debug.Log($"Security system power: {(isPowered ? "ON" : "OFF")}");
        
        // Sync with other button
        if (other_button != null)
        {
            other_button.isPowered = isPowered;
            if (other_button.monitorRenderer != null)
            {
                other_button.monitorRenderer.material = isPowered ? 
                    other_button.originalMaterial : monitorOffMaterial;
            }
            other_button.UpdateCameraStates();
        }
    }

    private void UpdateCameraStates()
    {
        if (!isPowered)
        {
            // When powered off, disable all cameras
            DisableAllCameras();
        }
        else
        {
            // When powered on, enable the selected camera
            if (cameras != null && cameras.Count > 0)
            {
                // First disable all cameras
                DisableAllCameras();
                // Then enable the currently selected camera
                cameras[cameraSelected].SetActive(true);
                Debug.Log($"Powered on: Enabling camera {cameraSelected}");
            }
            else
            {
                Debug.LogError("No cameras available to enable!");
            }
        }
    }

    private void PlayButtonAnimation()
    {
        if (buttonAnim != null && !string.IsNullOrEmpty(buttonPressAnimationName))
        {
            try
            {
                buttonAnim.Play(buttonPressAnimationName);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to play button animation: {e.Message}");
            }
        }
    }

    public void nextCam()
    {
        if (!isPowered || cameras == null || cameras.Count == 0) return;

        PlayButtonAnimation();
        
        // Disable current camera
        cameras[cameraSelected].SetActive(false);
        
        // Calculate next camera index
        cameraSelected = (cameraSelected + 1) % cameras.Count;
        
        // Enable new camera
        cameras[cameraSelected].SetActive(true);
        
        // Sync with other button
        if (other_button != null)
        {
            other_button.cameraSelected = cameraSelected;
        }
        
        Debug.Log($"Switched to camera {cameraSelected}");
    }

    public void previousCam()
    {
        if (!isPowered || cameras == null || cameras.Count == 0) return;

        PlayButtonAnimation();
        
        // Disable current camera
        cameras[cameraSelected].SetActive(false);
        
        // Calculate previous camera index
        cameraSelected = (cameraSelected - 1 + cameras.Count) % cameras.Count;
        
        // Enable new camera
        cameras[cameraSelected].SetActive(true);
        
        // Sync with other button
        if (other_button != null)
        {
            other_button.cameraSelected = cameraSelected;
        }
        
        Debug.Log($"Switched to camera {cameraSelected}");
    }

    private void DisableAllCameras()
    {
        foreach (GameObject camera in cameras)
        {
            if (camera != null)
            {
                camera.SetActive(false);
            }
        }
    }

    public void Interact()
    {
        nextCam();
    }

    private void ForcePowerOff()
    {
        if (!isPowered) return;
        
        isPowered = false;
        UpdateCameraStates();
        
        if (monitorRenderer != null)
        {
            if (monitorOffMaterial != null && originalMaterial != null)
            {
                monitorRenderer.material = monitorOffMaterial;
            }
        }
        
        // Sync with other button
        if (other_button != null)
        {
            other_button.isPowered = false;
            if (other_button.monitorRenderer != null)
            {
                other_button.monitorRenderer.material = monitorOffMaterial;
            }
            other_button.UpdateCameraStates();
        }
    }

    // Method to recharge the battery (call this from a charging station or power source)
    public void RechargeBattery(float amount)
    {
        currentBattery = Mathf.Min(maxBattery, currentBattery + amount);
        Debug.Log($"Battery recharged. Current level: {currentBattery:F1}%");
    }

    // Method to get current battery percentage
    public float GetBatteryPercentage()
    {
        return (currentBattery / maxBattery) * 100f;
    }
}
