using PUN;
using UI.Dialogs;

namespace Stations.Zones.Home
{
    public class HelpDeskZone : InteractableWithDialog<HelpDeskDialog>
    {
        protected override ZoneType ZoneType => ZoneType.HelpDesk;
        protected override string ActionSpriteName => "HelpDesk";

        protected override void StartAction()
        {
            StartCooldown();
            WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnHelpDeskOpened, GameEventResult.Success);
            CurrentDialog = WorldManager.canvasHandler.InitializeHelpDeskDialog(OnCloseDialog);
        }
    }
}