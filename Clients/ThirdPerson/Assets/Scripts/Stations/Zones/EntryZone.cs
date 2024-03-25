using Backend;
using PUN;
using UnityEngine;

namespace Stations.Zones
{
    public class EntryZone : Interactable
    {
        public GameObject exitZoneReference;

        protected override ZoneType ZoneType => ZoneType.Entry;
        protected override string ActionSpriteName => "door_open";

        protected override void StartAction()
        {
            StartCooldown();

            StartCoroutine(BackendConnection.VisitStation(response =>
            {
                if (HandlePossibleErrorResponse(response, OnActionFinished))
                {
                    ReactToGenericBoolResponse(response, () =>
                        {
                            WorldManager.ClientManager.FadeToStation(Station, exitZoneReference.transform.position);
                            WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnStationEntered,
                                GameEventResult.Success, Station.Type);
                            OnActionFinished();
                        }, () =>
                        {
                            WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnStationEntered,
                                GameEventResult.NotAllowed, Station.Type);
                            ShowNotAllowedMessage($"You are not allowed to enter {Station.Type} right now.",
                                OnActionFinished);
                        }
                    );
                }
                else
                {
                    WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnStationEntered,
                        GameEventResult.Error,
                        Station.Type);
                }
            }, Station.Type));
        }
    }
}