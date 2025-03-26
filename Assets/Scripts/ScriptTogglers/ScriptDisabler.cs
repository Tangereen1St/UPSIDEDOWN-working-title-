using UnityEngine;
using System.Collections;

public class ScriptDisabler : MonoBehaviour
{
    // Reference to the GameObject that contains the script to disable
    public GameObject targetObject;
    
    // The name of the script to disable
    public string scriptToDisable;
    
    // Time in seconds before the script is disabled
    public float disableDelay = 5f;
    
    // Option to use coroutine (allows for pausing) or just Invoke
    public bool useCoroutine = true;
    
    void Start()
    {
        if (targetObject == null)
        {
            Debug.LogError("Target object not assigned in ScriptDisabler!");
            return;
        }
        
        if (string.IsNullOrEmpty(scriptToDisable))
        {
            Debug.LogError("Script name not specified in ScriptDisabler!");
            return;
        }
        
        if (useCoroutine)
        {
            StartCoroutine(DisableScriptAfterDelay());
        }
        else
        {
            Invoke("DisableScript", disableDelay);
        }
    }
    
    IEnumerator DisableScriptAfterDelay()
    {
        yield return new WaitForSeconds(disableDelay);
        DisableScript();
    }
    
    void DisableScript()
    {
        // Try to find the component by its name
        Component targetScript = targetObject.GetComponent(scriptToDisable);
        
        if (targetScript != null)
        {
            // Disable the script
            MonoBehaviour scriptAsBehaviour = targetScript as MonoBehaviour;
            if (scriptAsBehaviour != null)
            {
                scriptAsBehaviour.enabled = false;
                Debug.Log($"Script '{scriptToDisable}' has been disabled on {targetObject.name}");
            }
            else
            {
                Debug.LogError($"Component '{scriptToDisable}' is not a MonoBehaviour and cannot be disabled!");
            }
        }
        else
        {
            Debug.LogError($"Could not find script '{scriptToDisable}' on {targetObject.name}!");
        }
    }
} 