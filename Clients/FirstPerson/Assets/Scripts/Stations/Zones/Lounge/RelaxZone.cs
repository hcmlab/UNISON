using System.Globalization;
using Backend;
using PUN;
using UI.Dialogs;
using UnityEngine;

namespace Stations.Zones.Lounge
{
    public class RelaxZone : InteractableWithDialog<YesNoDialog>
    {
        protected override ZoneType ZoneType => ZoneType.Relax;
        protected override string ActionSpriteName => "Relax";

        protected override void StartAction()
        {
            StartCoroutine(BackendConnection.GetScale(response =>
            {
                if (HandlePossibleErrorResponse(response, OnActionFinished))
                {
                    if (response.TryGetResponseAsDouble(out double loungeCostMoneyUnits))
                    {
                        OpenRelaxDialog(loungeCostMoneyUnits);
                    }
                    else
                    {
                        throw new UnityException($"Cannot get cost to relax: {response.GetRawResponse()}");
                    }
                }
                else
                {
                    WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnRelaxed, GameEventResult.Error);
                }
            }, Scale.CostLounge));
        }

        private void OpenRelaxDialog(double loungeCostMoneyUnits)
        {
            string moneyUnitsAsString = loungeCostMoneyUnits.ToString("N2", CultureInfo.InvariantCulture);
            CurrentDialog = WorldManager.canvasHandler.InitializeYesNoDialog(
                LocalizationUtility.GetLocalizedString("relaxTitle"),
                LocalizationUtility.GetLocalizedString("relaxMessage", new[] { moneyUnitsAsString }),
                LocalizationUtility.GetLocalizedString("relax"),
                LocalizationUtility.GetLocalizedString("cancel"),
                Relax,
                OnCloseDialog
            );
        }

        private void Relax()
        {
            CurrentDialog.ToggleButtons(false, CurrentDialog.yesButton);

            StartCoroutine(BackendConnection.Relax(response =>
            {
                CloseInfoMessage();

                if (HandlePossibleErrorResponse(response, OnActionFinished))
                {
                    if (response.TryGetResponseAsInt(out int relaxValue))
                    {
                        StartCooldown();

                        string relaxSuccessMessage = LocalizationUtility.GetLocalizedString("relaxSuccessMessage") +
                                                     " " + LocalizationUtility.GetLocalizedString(
                                                         $"relax{relaxValue}SuccessMessage");

                        WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnRelaxed,
                            GameEventResult.Success);
                        ShowSuccessMessage(relaxSuccessMessage, OnActionFinished);
                    }
                    else
                    {
                        WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnRelaxed,
                            GameEventResult.NotAllowed);
                        ShowNotAllowedMessage(LocalizationUtility.GetLocalizedString("relaxNotAllowedMessage"),
                            OnActionFinished);
                    }
                }
                else
                {
                    WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnRelaxed, GameEventResult.Error);
                }
            }));
        }
    }
}
