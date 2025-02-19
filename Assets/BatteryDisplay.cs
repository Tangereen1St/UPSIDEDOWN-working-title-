using UnityEngine;
using TMPro;

public class BatteryDisplay : MonoBehaviour
{
    public security_cam_system securitySystem;
    public TextMeshProUGUI batteryText;
    
    void Update()
    {
        if (securitySystem != null && batteryText != null)
        {
            batteryText.text = $"Battery: {securitySystem.GetBatteryPercentage():F1}%";
            
            // Optional: Change color based on battery level
            if (securitySystem.currentBattery < securitySystem.minimumBatteryToOperate)
            {
                batteryText.color = Color.red;
            }
            else if (securitySystem.currentBattery < securitySystem.maxBattery * 0.25f)
            {
                batteryText.color = Color.yellow;
            }
            else
            {
                batteryText.color = Color.green;
            }
        }
    }
} 