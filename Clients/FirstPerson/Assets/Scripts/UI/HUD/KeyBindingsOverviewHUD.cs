using Settings;
using TMPro;
using UI.Menu;
using UnityEngine;
using UnityEngine.UI;
using World;

namespace UI.HUD
{
    public class KeyBindingsOverviewHUD : MonoBehaviourHUD
    {
        [SerializeField] private CanvasGroup canvasGroup;

        [SerializeField] private Button primaryForwardButton;
        [SerializeField] private TMP_Text primaryForwardText;
        [SerializeField] private Button secondaryForwardButton;
        [SerializeField] private TMP_Text secondaryForwardText;

        [SerializeField] private Button primaryLeftButton;
        [SerializeField] private TMP_Text primaryLeftText;
        [SerializeField] private Button secondaryLeftButton;
        [SerializeField] private TMP_Text secondaryLeftText;

        [SerializeField] private Button primaryRightButton;
        [SerializeField] private TMP_Text primaryRightText;
        [SerializeField] private Button secondaryRightButton;
        [SerializeField] private TMP_Text secondaryRightText;

        [SerializeField] private Button primaryBackwardButton;
        [SerializeField] private TMP_Text primaryBackwardText;
        [SerializeField] private Button secondaryBackwardButton;
        [SerializeField] private TMP_Text secondaryBackwardText;

        [SerializeField] private Button interactButton;
        [SerializeField] private TMP_Text interactText;

        [SerializeField] private Button toggleVoiceButton;
        [SerializeField] private TMP_Text toggleVoiceText;
        
        [SerializeField] private Button openMapButton;
        [SerializeField] private TMP_Text openMapText;

        public static bool KeyBindingsChanged { get; set; }

        public override void OnBeingEnabled(WorldManager worldManager)
        {
            base.OnBeingEnabled(worldManager);
            
            KeyBindingsChanged = true;
        }

        private void Update()
        {
            if (SettingsMenu.KeyBindingsOverviewEnabled)
            {
                canvasGroup.alpha = 1;
                canvasGroup.interactable = true;

                if (KeyBindingsChanged)
                {
                    primaryForwardText.text = LocalizationUtility.GetTextForKeyCode(ControlSettings.ControlData.PrimaryForward);
                    secondaryForwardText.text = LocalizationUtility.GetTextForKeyCode(ControlSettings.ControlData.SecondaryForward);

                    primaryLeftText.text = LocalizationUtility.GetTextForKeyCode(ControlSettings.ControlData.PrimaryLeft);
                    secondaryLeftText.text = LocalizationUtility.GetTextForKeyCode(ControlSettings.ControlData.SecondaryLeft);
                    
                    primaryRightText.text = LocalizationUtility.GetTextForKeyCode(ControlSettings.ControlData.PrimaryRight);
                    secondaryRightText.text = LocalizationUtility.GetTextForKeyCode(ControlSettings.ControlData.SecondaryRight);
                    
                    primaryBackwardText.text = LocalizationUtility.GetTextForKeyCode(ControlSettings.ControlData.PrimaryBackward);
                    secondaryBackwardText.text = LocalizationUtility.GetTextForKeyCode(ControlSettings.ControlData.SecondaryBackward);
                    
                    interactText.text = LocalizationUtility.GetTextForKeyCode(ControlSettings.ControlData.Interact);
                    toggleVoiceText.text = LocalizationUtility.GetTextForKeyCode(ControlSettings.ControlData.ToggleVoice);
                    openMapText.text = LocalizationUtility.GetTextForKeyCode(ControlSettings.ControlData.OpenMap);

                    KeyBindingsChanged = false;
                }
                
                primaryForwardButton.interactable = Input.GetKey(ControlSettings.ControlData.PrimaryForward);
                secondaryForwardButton.interactable = Input.GetKey(ControlSettings.ControlData.SecondaryForward);

                primaryLeftButton.interactable = Input.GetKey(ControlSettings.ControlData.PrimaryLeft);
                secondaryLeftButton.interactable = Input.GetKey(ControlSettings.ControlData.SecondaryLeft);
                
                primaryRightButton.interactable = Input.GetKey(ControlSettings.ControlData.PrimaryRight);
                secondaryRightButton.interactable = Input.GetKey(ControlSettings.ControlData.SecondaryRight);
                
                primaryBackwardButton.interactable = Input.GetKey(ControlSettings.ControlData.PrimaryBackward);
                secondaryBackwardButton.interactable = Input.GetKey(ControlSettings.ControlData.SecondaryBackward);
                
                interactButton.interactable = Input.GetKey(ControlSettings.ControlData.Interact);
                toggleVoiceButton.interactable = Input.GetKey(ControlSettings.ControlData.ToggleVoice);
                openMapButton.interactable = Input.GetKey(ControlSettings.ControlData.OpenMap);
            }
            else
            {
                canvasGroup.alpha = 0;
                canvasGroup.interactable = false;
            }
        }
    }
}