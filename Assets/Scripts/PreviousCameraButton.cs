using UnityEngine;

public class PreviousCameraButton : MonoBehaviour, IInteractable
{
    public security_cam_system cameraSystem;

    public void Interact()
    {
        if (cameraSystem != null)
        {
            cameraSystem.previousCam();
        }
        else
        {
            Debug.LogError("Camera system not assigned to " + gameObject.name);
        }
    }
} 