using System.Globalization;
using Backend;
using PUN;
using UI.Dialogs;
using UnityEngine;

namespace Stations.Zones.Office
{
    public class EarnMoneyZone : InteractableWithDialog<YesNoDialog>
    {
        protected override ZoneType ZoneType => ZoneType.EarnMoney;
        protected override string ActionSpriteName => "EarnMoney";

        protected override void StartAction()
        {
            StartCoroutine(BackendConnection.GetCurrentPlayer(response =>
            {
                if (HandlePossibleErrorResponse(response, OnActionFinished))
                {
                    if (response.Data.HasLearned)
                    {
                        WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnMoneyEarned,
                            GameEventResult.NotAllowed);
                        ShowNotAllowedMessage(LocalizationUtility.GetLocalizedString("alreadyLearnedWarning"),
                            OnActionFinished);
                    }
                    else if (response.Data.HasWorked)
                    {
                        WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnMoneyEarned,
                            GameEventResult.NotAllowed);
                        ShowNotAllowedMessage(LocalizationUtility.GetLocalizedString("alreadyWorkedWarning"),
                            OnActionFinished);
                    }
                    else
                    {
                        OpenEarnMoneyDialog();
                    }
                }
                else
                {
                    WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnMoneyEarned,
                        GameEventResult.Error);
                }
            }, true));
        }

        private void OpenEarnMoneyDialog()
        {
            CurrentDialog = WorldManager.canvasHandler.InitializeYesNoDialog(
                LocalizationUtility.GetLocalizedString("earnMoneyTitle"),
                LocalizationUtility.GetLocalizedString("earnMoneyMessage"),
                LocalizationUtility.GetLocalizedString("yes"),
                LocalizationUtility.GetLocalizedString("no"),
                EarnMoney,
                OnCloseDialog
            );
        }

        private void EarnMoney()
        {
            CurrentDialog.ToggleButtons(false, CurrentDialog.yesButton);

            StartCoroutine(BackendConnection.EarnMoney(response =>
            {
                CloseInfoMessage();

                if (HandlePossibleErrorResponse(response))
                {
                    if (response.TryGetResponseAsDouble(out var moneyUnits))
                    {
                        StartCooldown();

                        var moneyUnitsAsString = moneyUnits.ToString("N2", CultureInfo.InvariantCulture);

                        WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnMoneyEarned,
                            GameEventResult.Success, moneyUnitsAsString);
                        ShowSuccessMessage(
                            LocalizationUtility.GetLocalizedString("earnMoneySuccessMessage",
                                new[] { moneyUnitsAsString }), OnActionFinished);
                    }
                    else if (response.TryGetResponseAsBool(out var success))
                    {
                        if (success)
                        {
                            throw new UnityException("Success response doesn't make sense here!");
                        }

                        WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnMoneyEarned,
                            GameEventResult.NotAllowed);
                        ShowNotAllowedMessage(LocalizationUtility.GetLocalizedString("earnMoneyNotAllowedMessage"),
                            OnActionFinished);
                    }
                    else
                    {
                        throw new UnityException($"Response \"{response.GetRawResponse()}\" not a bool or int!");
                    }
                }
                else
                {
                    WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnMoneyEarned,
                        GameEventResult.Error);
                }
            }, true));
        }
    }
}