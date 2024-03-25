using System;
using System.Collections;
using System.Globalization;
using Backend;
using Effects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using World;

namespace UI.HUD
{
    /// <summary>
    ///     Shows the health status, the money units and the quarantine status of the player
    /// </summary>
    public class HealthPointsHUD : MonoBehaviourHUD
    {
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private TMP_Text moneyUnitsText;

        [SerializeField] private Image heartImage;

        [SerializeField] private Sprite fullHeartSprite;
        [SerializeField] private Sprite halfFullHeartSprite;
        [SerializeField] private Sprite emptyHeartSprite;

        [SerializeField] private PulseEffect heartPulse;

        [SerializeField] private GameObject fadeDropTextPrefab;

        [SerializeField] private Image quarantineImage;

        [SerializeField] private Sprite quarantineSprite;
        [SerializeField] private Sprite nonQuarantineSprite;

        [SerializeField] private Color quarantineColor;
        [SerializeField] private Color nonQuarantineColor;

        private double lastMoneyUnits;

        public override void OnBeingEnabled(WorldManager worldManager)
        {
            base.OnBeingEnabled(worldManager);

            quarantineImage.sprite = quarantineSprite;
            StartCoroutine(UpdatePlayerInfo());
        }

        private IEnumerator UpdatePlayerInfo()
        {
            while (isActiveAndEnabled)
            {
                yield return UpdateQuarantine();
                yield return UpdateMoney();
                yield return UpdateHealth();
                yield return new WaitForSeconds(0.5f);
            }
        }

        private IEnumerator UpdateQuarantine()
        {
            yield return BackendConnection.GetPlayerProperty(response =>
            {
                if (response.IsSuccess() && response.TryGetResponseAsBool(out var isInQuarantine))
                {
                    if (isInQuarantine)
                    {
                        quarantineImage.sprite = quarantineSprite;
                        quarantineImage.color = quarantineColor;
                    }
                    else
                    {
                        quarantineImage.sprite = nonQuarantineSprite;
                        quarantineImage.color = nonQuarantineColor;
                    }
                }
                else
                {
                    Debug.LogError($"Error when updating quarantine HUD: \"{response.GetRawResponse()}\"");
                }
            }, PlayerProperty.IsInQuarantine);
        }

        private IEnumerator UpdateMoney()
        {
            yield return BackendConnection.GetPlayerProperty(response =>
            {
                if (response.IsSuccess() && response.TryGetResponseAsDouble(out var moneyUnits))
                {
                    SetMoneyText(moneyUnits);
                }
                else
                {
                    Debug.LogError($"Error when updating money HUD: \"{response.GetRawResponse()}\"");
                }
            }, PlayerProperty.MoneyUnits);
        }

        private IEnumerator UpdateHealth()
        {
            yield return BackendConnection.GetHealthStatus(response =>
            {
                if (response.IsSuccess() && response.TryGetResponseAsInt(out var healthStatus))
                {
                    switch (healthStatus)
                    {
                        case 0:
                            heartImage.sprite = emptyHeartSprite;
                            SetHealthText(LocalizationUtility.GetLocalizedString("healthStatusCritical"));
                            heartPulse.SetSpeedNormal();
                            break;
                        case 1:
                            heartImage.sprite = halfFullHeartSprite;
                            SetHealthText(LocalizationUtility.GetLocalizedString("healthStatusProblematic"));
                            heartPulse.SetSpeedSlow();
                            break;
                        case 2:
                            heartImage.sprite = fullHeartSprite;
                            SetHealthText(LocalizationUtility.GetLocalizedString("healthStatusGood"));
                            heartPulse.SetSpeedSlow();
                            break;

                        default:
                            Debug.LogError($"Unknown health status \"{healthStatus}\"!");
                            break;
                    }
                }
                else
                {
                    Debug.LogError($"Error when updating health points HUD: \"{response.GetRawResponse()}\"");
                }
            });
        }

        private void SetMoneyText(double moneyUnits)
        {
            var moneyUnitsDifference = moneyUnits - lastMoneyUnits;
            if (Math.Abs(moneyUnitsDifference) > 0.001)
            {
                var instantiatedText = Instantiate(fadeDropTextPrefab, gameObject.transform);
                var fadeDropText = instantiatedText.GetComponent<FadeDropText>();

                var moneyUnitsDifferenceAsString = moneyUnits.ToString("N2", CultureInfo.InvariantCulture);

                fadeDropText.SetTextColorPosition(
                    LocalizationUtility.GetLocalizedString("currency",
                        new[] { moneyUnitsDifferenceAsString }),
                    moneyUnitsDifference > 0 ? Color.green : Color.red,
                    moneyUnitsText.transform.position
                );
            }

            lastMoneyUnits = moneyUnits;
            var moneyUnitsAsString = moneyUnits.ToString("N2", CultureInfo.InvariantCulture);
            moneyUnitsText.text = LocalizationUtility.GetLocalizedString("currency", new[] { moneyUnitsAsString });
        }

        private void SetHealthText(string text)
        {
            healthText.text = text;
        }
    }
}