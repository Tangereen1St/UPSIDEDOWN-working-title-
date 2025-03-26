using UnityEngine;
using System.Collections;

public class ScriptDisablerGeneric<T> : MonoBehaviour where T : MonoBehaviour
{
    // Reference to the script to disable
    public T targetScript;
    
    // Time in seconds before the script is disabled
    public float disableDelay = 5f;
    
    // Option to use coroutine (allows for pausing) or just Invoke
    public bool useCoroutine = true;
    
    void Start()
    {
        if (targetScript == null)
        {
            Debug.LogError("Target script not assigned in ScriptDisablerGeneric!");
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
        if (targetScript != null)
        {
            targetScript.enabled = false;
            Debug.Log($"Script '{typeof(T).Name}' has been disabled on {targetScript.gameObject.name}");
        }
        else
        {
            Debug.LogError("Target script is null and cannot be disabled!");
        }
    }
} 