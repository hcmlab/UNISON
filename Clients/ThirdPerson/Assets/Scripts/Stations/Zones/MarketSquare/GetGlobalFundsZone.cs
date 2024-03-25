using System.Globalization;
using Backend;
using PUN;
using UI.Dialogs;

namespace Stations.Zones.MarketSquare
{
    public class GetGlobalFundsZone : InteractableWithDialog<OkDialog>
    {
        protected override ZoneType ZoneType => ZoneType.GetGlobalFunds;
        protected override string ActionSpriteName => "GetGlobalFunds";

        protected override void StartAction()
        {
            StartCooldown();

            StartCoroutine(BackendConnection.GetGlobalFunds(response =>
            {
                if (HandlePossibleErrorResponse(response, OnActionFinished))
                {
                    WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnGlobalFundsRequested,
                        GameEventResult.Success);

                    CurrentDialog = WorldManager.canvasHandler.InitializeOkDialog(
                        LocalizationUtility.GetLocalizedString("globalFundsTitle"),
                        LocalizationUtility.GetLocalizedString("globalFundsMessage", new[]
                        {
                            response.Data.VaccinationFund.ToString("N2", CultureInfo.InvariantCulture),
                            response.Data.Stocks.ToString("N2", CultureInfo.InvariantCulture),
                            response.Data.TaxAmount.ToString("N2", CultureInfo.InvariantCulture)
                        }),
                        LocalizationUtility.GetLocalizedString("close"),
                        OnCloseDialog
                    );
                }
                else
                {
                    WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnGlobalFundsRequested,
                        GameEventResult.Error);
                }
            }));
        }
    }
}