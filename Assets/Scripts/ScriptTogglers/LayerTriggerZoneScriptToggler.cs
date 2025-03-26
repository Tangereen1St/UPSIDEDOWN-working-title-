using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class LayerTriggerZoneScriptToggler : MonoBehaviour
{
    // Layer mask to determine which objects can trigger the zone
    public LayerMask targetLayers;
    
    // The component to toggle when the target enters/exits the trigger
    public MonoBehaviour targetComponent;
    
    // Whether to enable or disable the script when an object enters
    public bool enableOnEnter = false;
    
    // Whether to toggle when objects exit
    public bool toggleOnExit = false;
    
    // Whether the script is enabled when the scene starts
    public bool startEnabled = true;
    
    // Optional delay before toggling the script
    public float toggleDelay = 0f;
    
    // Whether to toggle the script only when a specific number of objects are in the zone
    public bool useObjectCountThreshold = false;
    
    // Number of objects required to toggle the script (if useObjectCountThreshold is true)
    public int objectCountThreshold = 1;
    
    // List to track objects currently in the trigger zone
    private List<Collider> objectsInTrigger = new List<Collider>();
    
    private Collider triggerCollider;
    
    void Start()
    {
        // Get the collider and ensure it's set as a trigger
        triggerCollider = GetComponent<Collider>();
        if (!triggerCollider.isTrigger)
        {
            Debug.LogWarning("Collider is not set as a trigger! Setting it now.");
            triggerCollider.isTrigger = true;
        }
        
        // Validate the target component
        if (targetComponent == null)
        {
            Debug.LogError("Target component not assigned in LayerTriggerZoneScriptToggler!");
        }
        else
        {
            // Set the initial state of the component
            targetComponent.enabled = startEnabled;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Check if the object's layer is in our target layers
        if (IsInTargetLayer(other.gameObject) && targetComponent != null)
        {
            // Add the object to our tracking list
            if (!objectsInTrigger.Contains(other))
            {
                objectsInTrigger.Add(other);
            }
            
            // If we're using a threshold, check if we've reached it
            if (!useObjectCountThreshold || objectsInTrigger.Count >= objectCountThreshold)
            {
                if (toggleDelay <= 0)
                {
                    SetComponentState(enableOnEnter);
                }
                else
                {
                    CancelInvoke("DelayedToggleExit");
                    Invoke("DelayedToggleEnter", toggleDelay);
                }
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        // Check if the object's layer is in our target layers
        if (IsInTargetLayer(other.gameObject) && targetComponent != null)
        {
            // Remove the object from our tracking list
            objectsInTrigger.Remove(other);
            
            // If we're toggling on exit and our count is below threshold
            if (toggleOnExit && (!useObjectCountThreshold || objectsInTrigger.Count < objectCountThreshold))
            {
                if (toggleDelay <= 0)
                {
                    SetComponentState(!enableOnEnter);
                }
                else
                {
                    CancelInvoke("DelayedToggleEnter");
                    Invoke("DelayedToggleExit", toggleDelay);
                }
            }
        }
    }
    
    bool IsInTargetLayer(GameObject obj)
    {
        return ((1 << obj.layer) & targetLayers) != 0;
    }
    
    void DelayedToggleEnter()
    {
        SetComponentState(enableOnEnter);
    }
    
    void DelayedToggleExit()
    {
        SetComponentState(!enableOnEnter);
    }
    
    void SetComponentState(bool state)
    {
        targetComponent.enabled = state;
        Debug.Log($"Component '{targetComponent.GetType().Name}' on {targetComponent.gameObject.name} is now {(state ? "enabled" : "disabled")}");
    }
    
    // Clean up when the component is disabled or the GameObject is destroyed
    void OnDisable()
    {
        objectsInTrigger.Clear();
        CancelInvoke();
    }
} 