using System.Globalization;
using Backend;
using PUN;
using UI.Dialogs;
using UnityEngine;

namespace Stations.Zones.Hospital
{
    public class BuyCarePackageZone : InteractableWithDialog<YesNoDialog>
    {
        protected override ZoneType ZoneType => ZoneType.BuyCarePackage;
        protected override string ActionSpriteName => "BuyCarePackage";

        protected override void StartAction()
        {
            StartCoroutine(BackendConnection.GetCurrentPlayer(response =>
            {
                if (!HandlePossibleErrorResponse(response, OnActionFinished))
                {
                    return;
                }

                if (response.Data.BoughtCarePackage)
                {
                    ShowNotAllowedMessage(LocalizationUtility.GetLocalizedString("alreadyBoughtCarePackageWarning"),
                        OnActionFinished);
                }
                else
                {
                    switch (response.Data.DaysInHospital)
                    {
                        case 2:
                            OpenBuyCarePackageDialog();
                            break;

                        case 1:
                            ShowNotAllowedMessage(LocalizationUtility.GetLocalizedString("lastDayCarePackageWarning"),
                                OnActionFinished);
                            break;

                        default:
                            ShowServerErrorMessage(LocalizationUtility.GetLocalizedString("inHospitalError"),
                                OnActionFinished);
                            break;
                    }
                }
            }, true));
        }

        private void OpenBuyCarePackageDialog()
        {
            StartCoroutine(BackendConnection.GetScale(costCarePackageResponse =>
            {
                if (!HandlePossibleErrorResponse(costCarePackageResponse))
                {
                    return;
                }

                if (costCarePackageResponse.TryGetResponseAsDouble(out var costCarePackage))
                {
                    var moneyUnitsAsString = costCarePackage.ToString("N2", CultureInfo.InvariantCulture);

                    CurrentDialog = WorldManager.canvasHandler.InitializeYesNoDialog(
                        LocalizationUtility.GetLocalizedString("buyCarePackageTitle"),
                        LocalizationUtility.GetLocalizedString("buyCarePackageMessage", new[] { moneyUnitsAsString }),
                        LocalizationUtility.GetLocalizedString("buy"),
                        LocalizationUtility.GetLocalizedString("cancel"),
                        BuyCarePackage,
                        OnCloseDialog
                    );
                }
                else
                {
                    throw new UnityException(
                        $"Cannot work with response: {costCarePackageResponse.GetRawResponse()}");
                }
            }, Scale.CostCarePackage));
        }

        private void BuyCarePackage()
        {
            CurrentDialog.ToggleButtons(false, CurrentDialog.yesButton);

            StartCooldown();

            StartCoroutine(BackendConnection.BuyCarePackage(response =>
            {
                CloseInfoMessage();

                if (HandlePossibleErrorResponse(response, OnActionFinished))
                {
                    ReactToGenericBoolResponse(response,
                        () =>
                        {
                            WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnCarePackageBought,
                                GameEventResult.Success);
                            ShowSuccessMessage(LocalizationUtility.GetLocalizedString("buyCarePackageSuccessMessage"),
                                OnActionFinished);
                        },
                        () =>
                        {
                            WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnCarePackageBought,
                                GameEventResult.NotAllowed);
                            ShowNotAllowedMessage(
                                LocalizationUtility.GetLocalizedString("buyCarePackageNotAllowedMessage"),
                                OnActionFinished);
                        }
                    );
                }
                else
                {
                    WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnCarePackageBought,
                        GameEventResult.Error);
                }
            }));
        }
    }
}