using UnityEngine;
using System.Collections;

public class ComponentDisabler : MonoBehaviour
{
    // Reference to the component to disable
    public MonoBehaviour targetComponent;
    
    // Time in seconds before the component is disabled
    public float disableDelay = 5f;
    
    // Option to use coroutine (allows for pausing) or just Invoke
    public bool useCoroutine = true;
    
    void Start()
    {
        if (targetComponent == null)
        {
            Debug.LogError("Target component not assigned in ComponentDisabler!");
            return;
        }
        
        if (useCoroutine)
        {
            StartCoroutine(DisableComponentAfterDelay());
        }
        else
        {
            Invoke("DisableComponent", disableDelay);
        }
    }
    
    IEnumerator DisableComponentAfterDelay()
    {
        yield return new WaitForSeconds(disableDelay);
        DisableComponent();
    }
    
    void DisableComponent()
    {
        if (targetComponent != null)
        {
            targetComponent.enabled = false;
            Debug.Log($"Component '{targetComponent.GetType().Name}' has been disabled on {targetComponent.gameObject.name}");
        }
        else
        {
            Debug.LogError("Target component is null and cannot be disabled!");
        }
    }
} 