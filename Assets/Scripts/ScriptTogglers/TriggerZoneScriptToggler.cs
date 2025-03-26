using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TriggerZoneScriptToggler : MonoBehaviour
{
    // The tag of the object that will trigger the script toggle
    public string targetTag = "Player";
    
    // The component to toggle when the target enters/exits the trigger
    public MonoBehaviour targetComponent;
    
    // Whether to enable or disable the script when the object enters
    public bool enableOnEnter = false;
    
    // Whether to toggle on exit as well
    public bool toggleOnExit = false;
    
    // Whether the script is enabled when the scene starts
    public bool startEnabled = true;
    
    // Optional delay before toggling the script
    public float toggleDelay = 0f;
    
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
            Debug.LogError("Target component not assigned in TriggerZoneScriptToggler!");
        }
        else
        {
            // Set the initial state of the component
            targetComponent.enabled = startEnabled;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag) && targetComponent != null)
        {
            if (toggleDelay <= 0)
            {
                SetComponentState(enableOnEnter);
            }
            else
            {
                Invoke("DelayedToggleEnter", toggleDelay);
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (toggleOnExit && other.CompareTag(targetTag) && targetComponent != null)
        {
            if (toggleDelay <= 0)
            {
                SetComponentState(!enableOnEnter);
            }
            else
            {
                Invoke("DelayedToggleExit", toggleDelay);
            }
        }
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
} 