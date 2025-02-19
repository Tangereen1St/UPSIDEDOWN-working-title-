using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class interaction : MonoBehaviour
{
    //The distance from which the player can interact with an object
    public float interactionDistance;

    //Text or crosshair that shows up to let the player know they can interact with an object they're looking at
    public GameObject interactionText;

    //Layers the raycast can hit/interact with. Any layers unchecked will be ignored by the raycast.
    public LayerMask interactionLayers;

    //The Update() void is used to make stuff happen every frame
    void Update()
    {
        // Test if E key works at all
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E key pressed globally");
        }

        RaycastHit hit;
        
        // Log the ray direction and distance
        Debug.DrawRay(transform.position, transform.forward * interactionDistance, Color.red);
        
        if(Physics.Raycast(transform.position, transform.forward, out hit, interactionDistance, interactionLayers))
        {
            Debug.Log($"Hit object: {hit.collider.gameObject.name} at distance {hit.distance}");
            
            IInteractable interactable = hit.collider.gameObject.GetComponent<IInteractable>();
            
            if (interactable != null)
            {
                Debug.Log($"Found interactable on {hit.collider.gameObject.name}");
                interactionText.SetActive(true);

                if (Input.GetKeyDown(KeyCode.E))
                {
                    Debug.Log($"E pressed while looking at {hit.collider.gameObject.name}");
                    interactable.Interact();
                }
            }
            else
            {
                Debug.Log($"No interactable component found on {hit.collider.gameObject.name}");
                interactionText.SetActive(false);
            }
        }
        else
        {
            interactionText.SetActive(false);
        }
    }
}
