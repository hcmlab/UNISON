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
    /// Shows the health status, the money units and the quarantine status of the player
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
                if (response.IsSuccess() && response.TryGetResponseAsBool(out bool isInQuarantine))
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
                    throw new UnityException($"Error when updating quarantine HUD: \"{response.GetRawResponse()}\"");
                }
            }, PlayerProperty.IsInQuarantine);
        }

        private IEnumerator UpdateMoney()
        {
            yield return BackendConnection.GetPlayerProperty(response =>
            {
                if (response.IsSuccess() && response.TryGetResponseAsDouble(out double moneyUnits))
                {
                    SetMoneyText(moneyUnits);
                }
                else
                {
                    throw new UnityException($"Error when updating money HUD: \"{response.GetRawResponse()}\"");
                }
            }, PlayerProperty.MoneyUnits);
        }

        private IEnumerator UpdateHealth()
        {
            yield return BackendConnection.GetHealthStatus(response =>
            {
                if (response.IsSuccess() && response.TryGetResponseAsInt(out int healthStatus))
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
                            throw new UnityException($"Unknown health status {healthStatus}!");
                    }
                }
                else
                {
                    throw new UnityException($"Error when updating health points HUD: \"{response.GetRawResponse()}\"");
                }
            });
        }

        private void SetMoneyText(double moneyUnits)
        {
            double moneyUnitsDifference = moneyUnits - lastMoneyUnits;
            if (Math.Abs(moneyUnitsDifference) > 0.001)
            {
                GameObject instantiatedText = Instantiate(fadeDropTextPrefab, gameObject.transform);
                FadeDropText fadeDropText = instantiatedText.GetComponent<FadeDropText>();

                string moneyUnitsDifferenceAsString = moneyUnits.ToString("N2", CultureInfo.InvariantCulture);

                fadeDropText.SetTextColorPosition(
                    LocalizationUtility.GetLocalizedString("currency",
                        new[] { moneyUnitsDifferenceAsString }),
                    moneyUnitsDifference > 0 ? Color.green : Color.red,
                    moneyUnitsText.transform.position
                );
            }

            lastMoneyUnits = moneyUnits;
            string moneyUnitsAsString = moneyUnits.ToString("N2", CultureInfo.InvariantCulture);
            moneyUnitsText.text = LocalizationUtility.GetLocalizedString("currency", new[] { moneyUnitsAsString });
        }

        private void SetHealthText(string text) => healthText.text = text;
    }
}