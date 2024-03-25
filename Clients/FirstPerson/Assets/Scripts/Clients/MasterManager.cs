using System;
using System.Collections;
using Backend;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using PUN;
using Settings;
using Stations;
using UI.Dialogs;
using UnityEngine;
using World;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Clients
{
    public enum NextRoundState
    {
        Open,
        NotReady,
        Ready,
        GameEnded
    }

    public class MasterManager : ClientManager
    {
        // references
        public override PunClient PunClient => PunMaster;
        [NonSerialized] public PunMaster PunMaster;

        // state
        public bool IsEveningEnded { get; set; }

        private NextRoundState nextRoundState;
        private bool isNextRoundStarted = true;
        private bool dayStateEventSent;

        [SerializeField] public float cameraDistance = 20f;
        [SerializeField] private float minCameraDistance = 10f;
        [SerializeField] private float maxCameraDistance = 30f;

        protected override void SetupClient()
        {
            PunMaster = Client.GetComponent<PunMaster>();
            focusOffset = WorldManager.focusObject.GetComponent<Offset>();
            focusOffset.Transform = PunClient.Transform;
            cameraOffset = WorldManager.mainCamera.GetComponent<Offset>();
            cameraOffset.offset.z = -cameraDistance;
            Station station = WorldManager.GetStationByType(StationType.City);
            SetStation(station);
        }

        protected override void UpdateClient()
        {
            UpdateDayState();
            HandleZoom();
        }

        private void UpdateDayState()
        {
            if (dayStateEventSent || WorldManager.RemainingDaytime > 0)
            {
                return;
            }

            int gameRound = WorldManager.GameRound;
            DayState dayState = WorldManager.DayState;
            switch (dayState)
            {
                case DayState.GameStarted:
                    return;

                case DayState.NextDay:
                    if (!WorldManager.NextDayFaded)
                    {
                        return;
                    }

                    switch (nextRoundState)
                    {
                        case NextRoundState.NotReady:
                            return;
                        case NextRoundState.Open:
                        case NextRoundState.Ready:
                            dayState = DayState.NewDay;
                            break;
                        case NextRoundState.GameEnded:
                            dayState = DayState.GameEnded;
                            break;
                    }

                    gameRound++;
                    break;

                case DayState.NewDay:
                    if (!isNextRoundStarted)
                    {
                        return;
                    }

                    nextRoundState = NextRoundState.Open;
                    dayState = DayState.Daylight;
                    break;

                case DayState.Daylight:
                    dayState = DayState.Dusk;
                    break;

                case DayState.Dusk:
                    dayState = DayState.Evening;
                    break;

                case DayState.Evening:
                    if (!IsEveningEnded)
                    {
                        return;
                    }

                    IsEveningEnded = false;
                    nextRoundState = NextRoundState.NotReady;
                    StartCoroutine(AdvanceToNextRound());
                    dayState = DayState.NextDay;
                    break;

                case DayState.GameEnded:
                    return;
            }

            WorldManager.SetRoomProperties(new Hashtable
            {
                { "gameRound", gameRound },
                { "dayState", dayState },
                { "daytimeProgress", 0.0f }
            });

            object[] content = { gameRound, dayState, 0.0f };
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent(WorldManager.UpdateDayStateCode, content, raiseEventOptions,
                SendOptions.SendReliable);
            dayStateEventSent = true;

            WorldManager.Save(PunClient.Name);
        }

        private IEnumerator AdvanceToNextRound()
        {
            yield return BackendConnection.AdvanceToNextRound(response =>
            {
                if (response.IsSuccess() && response.GetRawResponse() == "Vaccine was found")
                {
                    nextRoundState = NextRoundState.GameEnded;
                }
                else if (response.IsSuccess() && response.TryGetResponseAsBool(out _))
                {
                    nextRoundState = NextRoundState.Ready;
                }
                else
                {
                    throw new UnityException(
                        $"Error when advancing to the next round: \"{response.GetRawResponse()}\"");
                }
            });
        }

        public override void OnUpdated(DayState dayState, DayState oldDayState)
        {
            dayStateEventSent = false;
            switch (dayState)
            {
                case DayState.NewDay:
                    WorldManager.SetRoomProperties(new Hashtable { { "petitionUser", -1 } });

                    isNextRoundStarted = false;
                    WorldManager.canvasHandler.InitializeOkDialog(
                        LocalizationUtility.GetLocalizedString("dayEndedTitle"),
                        LocalizationUtility.GetLocalizedString("masterDayEndedMessage"),
                        LocalizationUtility.GetLocalizedString("ok"), FeedbackType.Neutral,
                        () => { isNextRoundStarted = true; });
                    break;
            }
        }

        protected override void HandleMovement(Vector3 movement)
        {
            Vector3 move = movement * gameSettings.SpectatorSpeed;
            if (Input.GetKey(ControlSettings.ControlData.Sprint))
                move *= gameSettings.SpectatorSprintMultiplier;

            PunMaster.Transform.position += move;
        }

        private void HandleZoom()
        {
            if (!CanInteract || IsClientMouseOnScrollingHUD)
            {
                return;
            }

            float scroll = Input.mouseScrollDelta.y;
            if (scroll == 0)
            {
                return;
            }

            cameraDistance = Mathf.Clamp(cameraDistance - scroll, minCameraDistance, maxCameraDistance);
            cameraOffset.offset.z = -cameraDistance;
        }

        protected override void SetPosition(Vector3 position)
        {
            PunMaster.Transform.position = position;
        }
    }
}