using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseSensitivity:MonoBehaviour
{
    public void UpdateMouseSensitivity(float sensitivity)
    {
        
        GameObject playerManager = GameObject.Find("PlayerManager(Clone)");
        if (playerManager != null)
        {
            Clients.PlayerManager manager = playerManager.GetComponent<Clients.PlayerManager>();
            if (GameObject.Find("SliderMouse").TryGetComponent<Slider>(out Slider slider))
            {
                StaticSettings.mouseSensitivity = StaticSettings.maxMouseSensitivity * slider.normalizedValue; ;
                manager.turnSpeed = StaticSettings.maxMouseSensitivity * slider.normalizedValue;
            } else {
                Debug.Log("Default Sensitivity");
                StaticSettings.mouseSensitivity = StaticSettings.defaultMouseSensitivity;
                manager.turnSpeed = StaticSettings.defaultMouseSensitivity;
            }
            
        }
    }
}
