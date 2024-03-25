using Backend;
using PUN;
using UnityEngine;

namespace Stations.Zones
{
    public class ExitZone : Interactable
    {
        public GameObject entryZoneReference;

        protected override ZoneType ZoneType => ZoneType.Exit;
        protected override string ActionSpriteName => "door_open";

        protected override void StartAction()
        {
            StartCooldown();

            StartCoroutine(BackendConnection.ExitStation(response =>
            {
                var oldStation = WorldManager.ClientManager.PunClient.CurrentStation;
                if (HandlePossibleErrorResponse(response, OnActionFinished))
                {
                    ReactToGenericBoolResponse(response, () =>
                        {
                            var station = WorldManager.GetStationByType(StationType.City);
                            WorldManager.ClientManager.FadeToStation(station, entryZoneReference.transform.position);
                            WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnStationLeft,
                                GameEventResult.Success, oldStation.Type);
                            OnActionFinished();
                        },
                        () =>
                        {
                            WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnStationLeft,
                                GameEventResult.NotAllowed, oldStation.Type);
                            ShowNotAllowedMessage($"You are not allowed to exit {Station.Type} right now.",
                                OnActionFinished);
                        }
                    );
                }
                else
                {
                    WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnStationLeft,
                        GameEventResult.Error,
                        oldStation.Type);
                }
            }, Station.Type));
        }
    }
}