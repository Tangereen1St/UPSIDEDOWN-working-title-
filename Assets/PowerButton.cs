using UnityEngine;

public class PowerButton : MonoBehaviour, IInteractable
{
    public security_cam_system cameraSystem;
    [Tooltip("Optional: Animator for button press animation")]
    public Animator buttonAnim;
    public string buttonPressAnimationName = "buttonpress";

    public void Interact()
    {
        if (cameraSystem != null)
        {
            PlayButtonAnimation();
            cameraSystem.TogglePower();
        }
        else
        {
            Debug.LogError("Camera system not assigned to " + gameObject.name);
        }
    }

    private void PlayButtonAnimation()
    {
        if (buttonAnim != null)
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
} 