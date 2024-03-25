using System;
using System.Globalization;
using Backend.ResponseDataCapsules;
using TMPro;
using UI.Auxiliary;
using UnityEngine;

namespace UI.HUD
{
    /// <summary>
    ///     A single element in the [PlayerStatsHUD]
    /// </summary>
    public class PlayerStatsListTile : MonoBehaviour
    {
        [SerializeField] private TMP_Text playerNameText;
        [SerializeField] private TMP_Text moneyUnitsText;
        [SerializeField] private TMP_Text healthPointsText;
        [SerializeField] private TMP_Text educationLevelText;
        [SerializeField] private TMP_Text learningSpeedText;
        [SerializeField] private TMP_Text stressLevelText;
        [SerializeField] private TMP_Text isIncubatingText;
        [SerializeField] private TMP_Text isInfectedText;
        [SerializeField] private CustomButton startButton;
        [SerializeField] private CustomButton stopButton;

        private DateTime? startedCreativeTime;

        public void SetPlayerProperties(Player player, bool isCreative, Action<int> onStartIsCreative,
            Action<int, Action> onStopIsCreative)
        {
            playerNameText.text = player.Name;
            moneyUnitsText.text = player.MoneyUnits.ToString("N2", CultureInfo.InvariantCulture);
            healthPointsText.text = player.HealthPoints.ToString("N2", CultureInfo.InvariantCulture);
            educationLevelText.text = player.EducationLevel.ToString("N2", CultureInfo.InvariantCulture);
            learningSpeedText.text = player.LearningSpeed.ToString("N2", CultureInfo.InvariantCulture);
            stressLevelText.text = player.StressLevel.ToString("N2", CultureInfo.InvariantCulture);
            isIncubatingText.text = player.IsIncubating
                ? LocalizationUtility.GetLocalizedString("yes")
                : LocalizationUtility.GetLocalizedString("no");
            isInfectedText.text = player.IsInfected
                ? LocalizationUtility.GetLocalizedString("yes")
                : LocalizationUtility.GetLocalizedString("no");

            startButton.UpdateButtonState(isCreative ? ButtonState.Hidden : ButtonState.Active);
            stopButton.UpdateButtonState(isCreative ? ButtonState.Active : ButtonState.Hidden);

            startButton.OnClick.AddListener(() =>
            {
                Debug.Log("On Start Button");

                startButton.UpdateButtonState(ButtonState.Hidden);
                onStartIsCreative(player.ID);
                stopButton.UpdateButtonState(ButtonState.Active);
            });

            stopButton.OnClick.AddListener(() =>
            {
                Debug.Log("On Stop Button");

                stopButton.UpdateButtonState(ButtonState.Loading);
                onStopIsCreative(player.ID, () =>
                {
                    stopButton.UpdateButtonState(ButtonState.Hidden);
                    startButton.UpdateButtonState(ButtonState.Active);
                });
            });
        }
    }
}