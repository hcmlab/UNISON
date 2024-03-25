using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Backend;
using PUN;
using UI.Dialogs;
using UnityEngine;
using UnityEngine.Events;

namespace Stations.Zones.Mall
{
    public class InvestZone : SpendZone<ToggleAndSpendDialog>
    {
        protected override ZoneType ZoneType => ZoneType.Invest;
        protected override string ActionSpriteName => "Invest";

        private static List<Fund> _availableFunds;
        private static List<string> _availableFundTexts;

        private const Fund DefaultFund = Fund.VaccinationFund;

        private void Awake()
        {
            _availableFunds = Enum.GetValues(typeof(Fund)).Cast<Fund>().ToList();
            _availableFundTexts = _availableFunds.Select(x => x.GetDisplayName()).ToList();
        }

        protected override void StartAction()
        {
            CurrentDialog = WorldManager.canvasHandler.InitializeToggleAndSpendDialog(
                LocalizationUtility.GetLocalizedString("investTitle"),
                LocalizationUtility.GetLocalizedString("investMessage"),
                LocalizationUtility.GetLocalizedString("invest"),
                LocalizationUtility.GetLocalizedString("cancel"),
                Invest,
                OnCloseDialog,
                GetLabelsAndOnValueChangedListeners(),
                DefaultFund.GetDisplayName()
            );

            UpdateFundValue(DefaultFund);
        }

        private List<(string, UnityAction<bool>)> GetLabelsAndOnValueChangedListeners()
        {
            List<(string, UnityAction<bool>)>
                labelsAndOnValueChangedListeners = new List<(string, UnityAction<bool>)>();
            for (int i = 0; i < _availableFundTexts.Count; ++i)
            {
                Fund fund = _availableFunds[i];
                string fundText = _availableFundTexts[i];

                labelsAndOnValueChangedListeners.Add((fundText, isOn =>
                {
                    if (isOn)
                    {
                        UpdateFundValue(fund);
                    }
                }));
            }

            return labelsAndOnValueChangedListeners;
        }

        private void UpdateFundValue(Fund fund)
        {
            switch (fund)
            {
                case Fund.VaccinationFund:
                    StartCoroutine(UpdateValueText(GameProperty.VaccinationFund));
                    break;

                case Fund.Stocks:
                    StartCoroutine(UpdateValueText(GameProperty.Stocks));
                    break;

                default:
                    throw new UnityException($"Unknown fund {fund}!");
            }
        }

        private IEnumerator UpdateValueText(GameProperty fund)
        {
            yield return StartCoroutine(BackendConnection.GetGameProperty(response =>
            {
                if (!HandlePossibleErrorResponse(response, OnActionFinished)) return;

                if (response.TryGetResponseAsDouble(out double moneyUnits))
                {
                    string moneyUnitsAsString = moneyUnits.ToString("N2", CultureInfo.InvariantCulture);

                    if (CurrentDialog)
                    {
                        switch (fund)
                        {
                            case GameProperty.VaccinationFund:
                                CurrentDialog.UpdateDialogMessage(
                                    LocalizationUtility.GetLocalizedString("investIntoVaccinationFundMessage",
                                        new[] { moneyUnitsAsString }));
                                break;

                            case GameProperty.Stocks:
                                CurrentDialog.UpdateDialogMessage(
                                    LocalizationUtility.GetLocalizedString("investIntoStocksMessage",
                                        new[] { moneyUnitsAsString }));
                                break;

                            default:
                                throw new UnityException($"Unknown fund {fund}!");
                        }
                    }
                }
            }, fund));
        }

        private void Invest()
        {
            if (!TryGetMoneyUnits(out double moneyUnits)) return;

            CurrentDialog.ToggleButtons(false, CurrentDialog.yesButton);
            Fund fund = _availableFunds[CurrentDialog.GetIndexOfSelection()];
            StartCooldown();

            switch (fund)
            {
                case Fund.VaccinationFund:
                    StartCoroutine(BackendConnection.InvestIntoVaccineFund(response =>
                    {
                        CloseInfoMessage();

                        if (HandlePossibleErrorResponse(response, OnActionFinished))
                        {
                            ReactToGenericBoolResponse(response,
                                () =>
                                {
                                    WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnInvested,
                                        GameEventResult.Success, fund, moneyUnits);
                                    ShowSuccessMessage(
                                        LocalizationUtility.GetLocalizedString(
                                            "investIntoVaccinationFundSuccessMessage"), OnActionFinished);
                                },
                                () =>
                                {
                                    WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnInvested,
                                        GameEventResult.NotAllowed, fund, moneyUnits);
                                    ShowSuccessMessage(
                                        LocalizationUtility.GetLocalizedString(
                                            "investIntoVaccinationFundNotAllowedMessage"), OnActionFinished);
                                }
                            );
                        }
                        else
                        {
                            WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnInvested,
                                GameEventResult.Error, fund, moneyUnits);
                        }
                    }, moneyUnits));
                    break;

                case Fund.Stocks:
                    StartCoroutine(BackendConnection.InvestIntoStocks(response =>
                    {
                        CloseInfoMessage();

                        if (HandlePossibleErrorResponse(response, OnActionFinished))
                        {
                            ReactToGenericBoolResponse(response,
                                () =>
                                {
                                    WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnInvested,
                                        GameEventResult.Success, fund, moneyUnits);
                                    ShowSuccessMessage(
                                        LocalizationUtility.GetLocalizedString("investIntoStocksSuccessMessage"),
                                        OnActionFinished);
                                },
                                () =>
                                {
                                    WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnInvested,
                                        GameEventResult.NotAllowed, fund, moneyUnits);
                                    ShowSuccessMessage(
                                        LocalizationUtility.GetLocalizedString("investIntoStocksNotAllowedMessage"),
                                        OnActionFinished);
                                }
                            );
                        }
                        else
                        {
                            WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnInvested,
                                GameEventResult.Error, fund, moneyUnits);
                        }
                    }, moneyUnits));
                    break;

                default:
                    throw new UnityException($"Unknown fund {fund}!");
            }
        }
    }
}