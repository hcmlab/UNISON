using Backend;
using PUN;
using UI.Dialogs;

namespace Stations.Zones.MarketSquare
{
    public class GetGraphsZone : InteractableWithDialog<GraphsDialog>
    {
        protected override ZoneType ZoneType => ZoneType.GetGraphs;
        protected override string ActionSpriteName => "GetGraphs";

        protected override void StartAction()
        {
            StartCooldown();

            if (WorldManager.GameRound > 1)
            {
                StartCoroutine(BackendConnection.GetGraphs(response =>
                {
                    if (HandlePossibleErrorResponse(response))
                    {
                        var availableGraphs = response.Data;
                        if (availableGraphs.Count == 0)
                        {
                            WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnGraphsRequested,
                                GameEventResult.NotAllowed);
                            ShowNotAllowedMessage(LocalizationUtility.GetLocalizedString("graphsNotAvailableWarning"),
                                OnActionFinished);
                        }
                        else
                        {
                            CurrentDialog = WorldManager.canvasHandler.InitializeGraphsDialog(
                                LocalizationUtility.GetLocalizedString("graphsTitle"),
                                LocalizationUtility.GetLocalizedString("graphsMessage"),
                                LocalizationUtility.GetLocalizedString("close"),
                                OnCloseDialog,
                                availableGraphs
                            );
                        }
                    }
                    else
                    {
                        WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnGraphsRequested,
                            GameEventResult.Error);
                    }
                }));
            }
            else
            {
                WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnGraphsRequested,
                    GameEventResult.NotAllowed);
                ShowNotAllowedMessage(LocalizationUtility.GetLocalizedString("graphsNotAvailableWarning"),
                    OnActionFinished);
            }
        }
    }
}