using System.Collections.Generic;
using Backend;
using PUN;
using UI.Dialogs;

namespace Stations.Zones.TownHall
{
    public class GetPetitionsZone : InteractableWithDialog<OkDialog>
    {
        protected override ZoneType ZoneType => ZoneType.GetPetitions;
        protected override string ActionSpriteName => "GetPetitions";

        protected override void StartAction()
        {
            StartCoroutine(BackendConnection.GetScales(scalesResponse =>
            {
                if (HandlePossibleErrorResponse(scalesResponse, OnActionFinished))
                {
                    StartCoroutine(BackendConnection.GetPetitions(petitionsResponse =>
                    {
                        if (HandlePossibleErrorResponse(petitionsResponse, OnActionFinished))
                        {
                            WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnPetitionsRequested,
                                GameEventResult.Success);

                            string petitionEffectsText = DialogUtility.CreatePetitionEffectsText(scalesResponse.Data, petitionsResponse.Data);

                            StartCooldown();

                            CurrentDialog = WorldManager.canvasHandler.InitializeBigOkDialog(
                                LocalizationUtility.GetLocalizedString("getPetitionsTitle"),
                                petitionEffectsText,
                                LocalizationUtility.GetLocalizedString("close"),
                                OnCloseDialog
                            );
                        }
                        else
                        {
                            WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnPetitionsRequested,
                                GameEventResult.Error);
                        }
                    }));
                }
                else
                {
                    WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnPetitionsRequested,
                        GameEventResult.Error);
                }
            }, DialogUtility.PetitionScales));
        }
    }
}
