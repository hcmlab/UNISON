using System;
using System.Collections;
using System.Collections.Generic;
using Backend;
using Photon.Pun;
using PUN;
using Settings;
using Stations;
using Stations.Zones;
using Stations.Zones.TownHall;
using UI.Dialogs;
using UnityEngine;
using World;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Clients
{
    public class PlayerManager : ClientManager
    {
        public float callForHelpCoolDown = 10f;

        public float respawnY;

        // references
        private YesNoDialog callForHelpDialog;
        private float callForHelpTime;

        // state
        private StationType eveningStationType = StationType.None;

        private Vector3 lastSetPosition = Vector3.zero;

        [NonSerialized] public PetitionZone PetitionZone;
        [NonSerialized] public PunPlayer PunPlayer;
        public Dictionary<int, bool> RegisterPetitionVoteByPlayerIDs;
        private bool respawning;
        private float turnSmoothVelocity;

        public override PunClient PunClient => PunPlayer;
        public float RemainingCallForHelpCooldown => callForHelpTime + callForHelpCoolDown - Time.time;
        public bool CallForHelpHasCooldown => RemainingCallForHelpCooldown > 0;

        protected override void SetupClient()
        {
            PunPlayer = Client.GetComponent<PunPlayer>();
            PunPlayer.CharacterController.enabled = true;
            WorldManager.minimapCamera.gameObject.SetActive(true);
            WorldManager.minimapCamera.GetComponent<Offset>().Transform = PunClient.Transform;
            eveningStationType = StationType.None;

            var station = WorldManager.GetStationByType(StationType.Home);
            SetStation(station);

            StartCoroutine(CheckHospitalization(LeaveCurrentAndTeleportToTargetStation));
        }

        protected override void UpdateClient()
        {
            if (CanInteract && Input.GetKeyDown(ControlSettings.ControlData.CallForHelp))
            {
                ShowCallForHelp();
            }

            HandleRespawn();
        }

        public override void OnUpdated(DayState dayState, DayState oldDayState)
        {
            switch (dayState)
            {
                case DayState.NewDay:
                    InteractableConfiguration.ResetCooldowns();

                    WorldManager.canvasHandler.InitializeSimpleDialog(
                        LocalizationUtility.GetLocalizedString("dayEndedTitle"),
                        LocalizationUtility.GetLocalizedString("playerDayEndedMessage"));

                    StartCoroutine(CheckHospitalization(LeaveCurrentAndTeleportToTargetStation));
                    break;

                case DayState.Daylight:
                    switch (PunClient.DaysHospitalized)
                    {
                        case 2:
                            PunClient.SendNewEvent(GameEventType.OnStartedDayInHospital, GameEventResult.Success, 2);

                            WorldManager.canvasHandler.InitializeOkDialog(
                                LocalizationUtility.GetLocalizedString("newDayTitle"),
                                LocalizationUtility.GetLocalizedString("twoDaysInHospitalMessage"),
                                LocalizationUtility.GetLocalizedString("ok"), FeedbackType.Neutral, null);
                            break;

                        case 1:
                            PunClient.SendNewEvent(GameEventType.OnStartedDayInHospital, GameEventResult.Success, 1);

                            WorldManager.canvasHandler.InitializeOkDialog(
                                LocalizationUtility.GetLocalizedString("newDayTitle"),
                                LocalizationUtility.GetLocalizedString("oneDayInHospitalMessage"),
                                LocalizationUtility.GetLocalizedString("ok"), FeedbackType.Neutral, null);
                            break;

                        case 0:
                            StartCoroutine(BackendConnection.GetOpenPetitions(response =>
                            {
                                if (response.IsSuccess())
                                {
                                    PunClient.SendNewEvent(GameEventType.OnReleasedFromHospital,
                                        GameEventResult.Success);

                                    var message =
                                        LocalizationUtility.GetLocalizedString("releasedFromHospitalMessage") + "\n" +
                                        LocalizationUtility.GetLocalizedString("openPetitionsMessage",
                                            new[] { response.Data.Count });

                                    WorldManager.canvasHandler.InitializeOkDialog(
                                        LocalizationUtility.GetLocalizedString("newDayTitle"), message,
                                        LocalizationUtility.GetLocalizedString("ok"), FeedbackType.Neutral, null);
                                }
                                else
                                {
                                    throw new UnityException($"Cannot get open petitions: {response.GetRawResponse()}");
                                }
                            }, true));
                            break;

                        case -1:
                            StartCoroutine(BackendConnection.GetOpenPetitions(response =>
                            {
                                if (response.IsSuccess())
                                {
                                    PunClient.SendNewEvent(GameEventType.OnStartedDayInHome, GameEventResult.Success);

                                    var message = LocalizationUtility.GetLocalizedString("newDayMessage") + "\n" +
                                                  LocalizationUtility.GetLocalizedString("openPetitionsMessage",
                                                      new[] { response.Data.Count });

                                    WorldManager.canvasHandler.InitializeOkDialog(
                                        LocalizationUtility.GetLocalizedString("newDayTitle"), message,
                                        LocalizationUtility.GetLocalizedString("ok"), FeedbackType.Neutral, null);
                                }
                                else
                                {
                                    throw new UnityException($"Cannot get open petitions: {response.GetRawResponse()}");
                                }
                            }, true));
                            break;

                        default:
                            throw new UnityException($"Invalid hospitalization state: {PunClient.DaysHospitalized}");
                    }

                    break;
                case DayState.Dusk:
                    if (PunClient.IsHospitalized)
                    {
                        return;
                    }

                    StartCoroutine(BackendConnection.GetScale(response =>
                    {
                        if (response.IsSuccess())
                        {
                            var isInLockdown = DeserializationUtility.StringToBool(response.GetRawResponse());

                            eveningStationType = WorldManager.DefaultEveningStationType;
                            WorldManager.canvasHandler.InitializeEveningActivityDialog(
                                () => eveningStationType = StationType.Home,
                                () => eveningStationType = StationType.TownHall,
                                () => eveningStationType = StationType.Lounge,
                                eveningStationType, WorldManager.DaytimeUpdateTime, isInLockdown
                            );
                        }
                        else
                        {
                            throw new UnityException(
                                $"Cannot check whether game is in lockdown: {response.GetRawResponse()}");
                        }
                    }, Scale.Lockdown, true));
                    break;

                case DayState.Evening:
                    if (PunClient.IsHospitalized)
                    {
                        return;
                    }

                    if (eveningStationType == StationType.None)
                    {
                        // Player just joined.
                        StartCoroutine(GoToPreviouslyChosenEveningStation());
                    }
                    else
                    {
                        if (!WorldManager.GetStationByType(eveningStationType).EveningValid)
                        {
                            PunPlayer.SendNewEvent(GameEventType.OnEveningStationSelected, GameEventResult.NotAllowed,
                                eveningStationType);
                            eveningStationType = WorldManager.DefaultEveningStationType;
                        }

                        StartCoroutine(GoToEveningStation());
                    }

                    break;
            }
        }

        private IEnumerator GoToPreviouslyChosenEveningStation()
        {
            yield return BackendConnection.GetPlayerProperty(response =>
            {
                if (response.IsSuccess())
                {
                    var eveningActivityStationType =
                        DeserializationUtility.GetStationTypeFromBackendName(response.GetRawResponse());

                    eveningStationType = eveningActivityStationType != StationType.None
                        ? eveningActivityStationType
                        : WorldManager.DefaultEveningStationType;
                }
                else
                {
                    throw new UnityException($"Cannot get evening activity station: {response.GetRawResponse()}");
                }
            }, PlayerProperty.EveningActivity, true);
            yield return GoToEveningStation();
        }

        private IEnumerator GoToEveningStation()
        {
            yield return BackendConnection.SaveEveningActivityStation(response =>
            {
                if (response.IsSuccess())
                {
                    PunPlayer.SendNewEvent(GameEventType.OnEveningStationSelected, GameEventResult.Success,
                        eveningStationType);
                    LeaveCurrentAndTeleportToTargetStation(eveningStationType);
                }
                else
                {
                    PunPlayer.SendNewEvent(GameEventType.OnEveningStationSelected, GameEventResult.Error,
                        eveningStationType);
                    throw new UnityException($"Cannot save evening activity station: {response.GetRawResponse()}");
                }
            }, eveningStationType, true);
        }

        public void ShowCallForHelp()
        {
            if (CallForHelpHasCooldown || callForHelpDialog)
            {
                return;
            }

            callForHelpDialog = WorldManager.canvasHandler.InitializeYesNoDialog(
                LocalizationUtility.GetLocalizedString("callForHelpTitle"),
                LocalizationUtility.GetLocalizedString("callForHelpMessage"),
                LocalizationUtility.GetLocalizedString("ok"), LocalizationUtility.GetLocalizedString("cancel"), () =>
                {
                    CallForHelp();
                    Destroy(callForHelpDialog.gameObject);
                }, () => { Destroy(callForHelpDialog.gameObject); });
        }

        private void CallForHelp()
        {
            callForHelpTime = Time.time;
            PunPlayer.RPC("OnCalledForHelp", RpcTarget.MasterClient);
        }

        public void NotifyNewPetitionEffect(int petitionID)
        {
            Debug.Log("NotifyNewPetitionEffect: " + petitionID);
            PunPlayer.RPC("OnNewPetitionEffect", RpcTarget.Others, petitionID);
        }

        public void StartNewRegisterPetitionVote(PetitionZone petitionZone, int petitionID)
        {
            PetitionZone = petitionZone;
            Debug.Log("StartNewRegisterPetitionVote: " + petitionID);

            RegisterPetitionVoteByPlayerIDs = new Dictionary<int, bool>();

            PunPlayer.RPC("OnNewRegisterPetitionVote", RpcTarget.All, petitionID);
            StartCoroutine(WaitForPetitionVotes());
        }

        private IEnumerator WaitForPetitionVotes()
        {
            yield return new WaitForSeconds(WorldManager.RegisterPetitionDialogDuration + 2f);
            var positiveVotes = 0;
            var negativeVotes = 0;
            foreach (var registerPetition in RegisterPetitionVoteByPlayerIDs.Values)
            {
                if (registerPetition)
                {
                    positiveVotes++;
                }
                else
                {
                    negativeVotes++;
                }
            }

            RegisterPetitionVoteByPlayerIDs = null;

            PunPlayer.RPC("OnRegisterPetitionVoteResult", RpcTarget.All, positiveVotes, negativeVotes);
        }

        public void SendRegisterPetitionVote(bool registerPetition, int playerID)
        {
            Debug.Log("SendRegisterPetitionVote: " + registerPetition + ", " + playerID);
            PunPlayer.RPC("OnRegisterPetitionVote", RpcTarget.All, registerPetition, playerID);
        }

        protected override void HandleMovement(Vector3 movement)
        {
            if (!PunPlayer.CharacterController.enabled || !PunPlayer.IsMoving)
            {
                return;
            }

            var targetAngle = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg;

            var angle = Mathf.SmoothDampAngle(PunPlayer.Transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity,
                gameSettings.TurnSmoothTime);
            PunPlayer.Transform.rotation = Quaternion.Euler(0, angle, 0);

            PunPlayer.CharacterController.SimpleMove(gameSettings.PlayerSpeed * movement);
        }

        private void HandleRespawn()
        {
            if (respawning || PunPlayer.Transform.position.y > respawnY)
            {
                return;
            }

            respawning = true;
            SetPosition(lastSetPosition);
            Debug.LogWarning("Player fell from the map and has been respawned!");
        }

        protected override void SetPosition(Vector3 position)
        {
            lastSetPosition = position;
            PunPlayer.CharacterController.enabled = false;

            PunPlayer.Transform.position = position;
            PunPlayer.CharacterController.enabled = true;

            respawning = false;
        }

        private IEnumerator CheckHospitalization(Action<StationType> afterCheck)
        {
            yield return BackendConnection.GetDaysRemainingInHospital(response =>
            {
                var stationType = StationType.Home;
                if (response.IsSuccess() && response.TryGetResponseAsInt(out var numberDays))
                {
                    PunClient.DaysHospitalized = numberDays switch
                    {
                        2 => 2,
                        1 => 1,
                        0 => PunClient.DaysHospitalized > 0 ? 0 : -1,
                        _ => throw new UnityException($"Invalid number of days in hospital: {numberDays}")
                    };

                    WorldManager.SetPlayerProperties(new Hashtable
                    {
                        { "daysHospitalized", PunClient.DaysHospitalized }
                    });

                    if (PunClient.IsHospitalized)
                    {
                        stationType = StationType.Hospital;
                    }

                    PunPlayer.AdjustRotation();
                }

                afterCheck.Invoke(stationType);
            });
        }

        private void LeaveCurrentAndTeleportToTargetStation(StationType targetStation)
        {
            if (PunPlayer.CurrentStationType == targetStation)
            {
                return;
            }

            if (!PunPlayer.CurrentStation.IsInterior)
            {
                TeleportToTargetStation(targetStation);
                return;
            }

            StartCoroutine(BackendConnection.ExitStation(response =>
            {
                if (response.IsSuccess())
                {
                    TeleportToTargetStation(targetStation);
                }
                else
                {
                    throw new UnityException(
                        $"Cannot leave current station {PunPlayer.CurrentStationType}: {response.GetRawResponse()}");
                }
            }, PunPlayer.CurrentStationType));
        }

        private void TeleportToTargetStation(StationType targetStation)
        {
            StartCoroutine(BackendConnection.VisitStation(response =>
            {
                if (response.IsSuccess())
                {
                    FadeToStation(targetStation);
                }
                else
                {
                    throw new UnityException(
                        $"Cannot visit evening station {targetStation}: {response.GetRawResponse()}");
                }
            }, targetStation));
        }
    }
}