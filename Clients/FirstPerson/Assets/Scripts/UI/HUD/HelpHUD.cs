using Clients;
using Settings;
using TMPro;
using UI.Auxiliary;
using UnityEngine;
using World;

namespace UI.HUD
{
    /// <summary>
    /// Shows the player the menu button and the help button. Via the help button, the player can call the master client for help
    /// </summary>
    public class HelpHUD : MonoBehaviourHUD
    {
        [SerializeField] private TMP_Text menuText;
        [SerializeField] private TMP_Text helpText;
        [SerializeField] private CircularProgressObject circularProgressObject;

        private PlayerManager playerManager;

        private bool hasCooldown;

        public override void OnBeingEnabled(WorldManager worldManager)
        {
            base.OnBeingEnabled(worldManager);

            playerManager = CurrentWorldManager.PlayerManager;
            circularProgressObject.gameObject.SetActive(hasCooldown);
            
            menuText.text = LocalizationUtility.AddLocalizedKeyCode(ControlSettings.ControlData.ToggleMenu,
                LocalizationUtility.GetLocalizedString("menu"));
            
            helpText.text = LocalizationUtility.AddLocalizedKeyCode(ControlSettings.ControlData.CallForHelp,
                LocalizationUtility.GetLocalizedString("help"));
        }

        private void Update()
        {
            hasCooldown = playerManager.CallForHelpHasCooldown;
            circularProgressObject.gameObject.SetActive(hasCooldown);

            if (hasCooldown)
            {
                float remainingCooldown = playerManager.RemainingCallForHelpCooldown;
                float percentage = remainingCooldown / playerManager.callForHelpCoolDown;
                circularProgressObject.SetPercentage(percentage);
                float rounded = Mathf.Ceil(remainingCooldown);
                circularProgressObject.SetRemainingSecondsText(rounded);
            }
        }

        public void OnMenuPressed()
        {
            CurrentWorldManager.canvasHandler.ToggleMenu(!CurrentWorldManager.canvasHandler.MenuOpened);
        }

        public void OnHelpPressed()
        {
            if (!hasCooldown)
            {
                playerManager.ShowCallForHelp();
            }
        }
    }
}