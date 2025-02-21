using UnityEngine;

public class NextCameraButton : MonoBehaviour, IInteractable
{
    public security_cam_system cameraSystem;

    public void Interact()
    {
        if (cameraSystem != null)
        {
            cameraSystem.nextCam();
        }
        else
        {
            Debug.LogError("Camera system not assigned to " + gameObject.name);
        }
    }
} 