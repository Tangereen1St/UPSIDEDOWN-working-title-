using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class MultiComponentTriggerToggler : MonoBehaviour
{
    [System.Serializable]
    public class ComponentToggleEntry
    {
        public MonoBehaviour targetComponent;
        public bool enableOnTrigger = false;
        public float toggleDelay = 0f;
    }
    
    // The tag of the object that will trigger the component toggles
    public string targetTag = "Player";
    
    // List of components to toggle and their settings
    public List<ComponentToggleEntry> componentsToToggle = new List<ComponentToggleEntry>();
    
    // Whether to toggle on exit as well (will reverse the enableOnTrigger value)
    public bool toggleOnExit = false;
    
    private Collider triggerCollider;
    private Dictionary<MonoBehaviour, float> pendingToggles = new Dictionary<MonoBehaviour, float>();
    private float timer = 0f;
    private bool isProcessingToggles = false;
    
    void Start()
    {
        // Get the collider and ensure it's set as a trigger
        triggerCollider = GetComponent<Collider>();
        if (!triggerCollider.isTrigger)
        {
            Debug.LogWarning("Collider is not set as a trigger! Setting it now.");
            triggerCollider.isTrigger = true;
        }
        
        // Validate the component entries
        foreach (ComponentToggleEntry entry in componentsToToggle)
        {
            if (entry.targetComponent == null)
            {
                Debug.LogError("A target component in MultiComponentTriggerToggler is not assigned!");
            }
        }
    }
    
    void Update()
    {
        // Process any pending toggles with delays
        if (isProcessingToggles)
        {
            timer += Time.deltaTime;
            List<MonoBehaviour> componentsToRemove = new List<MonoBehaviour>();
            
            foreach (var kvp in pendingToggles)
            {
                if (timer >= kvp.Value)
                {
                    // Find the entry for this component
                    ComponentToggleEntry matchingEntry = componentsToToggle.Find(entry => entry.targetComponent == kvp.Key);
                    if (matchingEntry != null)
                    {
                        kvp.Key.enabled = matchingEntry.enableOnTrigger;
                        Debug.Log($"Component '{kvp.Key.GetType().Name}' on {kvp.Key.gameObject.name} is now {(matchingEntry.enableOnTrigger ? "enabled" : "disabled")}");
                    }
                    componentsToRemove.Add(kvp.Key);
                }
            }
            
            // Remove processed components
            foreach (MonoBehaviour comp in componentsToRemove)
            {
                pendingToggles.Remove(comp);
            }
            
            // If no more pending toggles, stop processing
            if (pendingToggles.Count == 0)
            {
                isProcessingToggles = false;
            }
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            ToggleComponents(false);
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (toggleOnExit && other.CompareTag(targetTag))
        {
            ToggleComponents(true);
        }
    }
    
    void ToggleComponents(bool isExit)
    {
        // Clear any pending toggles
        pendingToggles.Clear();
        
        // Reset timer
        timer = 0f;
        
        foreach (ComponentToggleEntry entry in componentsToToggle)
        {
            if (entry.targetComponent != null)
            {
                bool targetState = isExit ? !entry.enableOnTrigger : entry.enableOnTrigger;
                
                if (entry.toggleDelay <= 0f)
                {
                    // Toggle immediately
                    entry.targetComponent.enabled = targetState;
                    Debug.Log($"Component '{entry.targetComponent.GetType().Name}' on {entry.targetComponent.gameObject.name} is now {(targetState ? "enabled" : "disabled")}");
                }
                else
                {
                    // Add to pending toggles
                    pendingToggles[entry.targetComponent] = entry.toggleDelay;
                    isProcessingToggles = true;
                }
            }
        }
    }
    
    void OnDisable()
    {
        pendingToggles.Clear();
        isProcessingToggles = false;
    }
} 