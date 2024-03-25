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

        // references
        private YesNoDialog callForHelpDialog;

        public override PunClient PunClient => PunPlayer;
        [NonSerialized] public PunPlayer PunPlayer;

        // state
        private StationType eveningStationType = StationType.None;

        public float callForHelpCoolDown = 10f;
        private float callForHelpTime;
        public float RemainingCallForHelpCooldown => (callForHelpTime + callForHelpCoolDown) - Time.time;
        public bool CallForHelpHasCooldown => RemainingCallForHelpCooldown > 0;

        public float respawnY;
        private float turnSmoothVelocity;

        private Vector3 lastSetPosition = Vector3.zero;
        private bool respawning;

        // movement
        private float gravity = -9.81f;
        private Vector3 fallVector = new Vector3(0f, 0f, 0f);
        private bool groundContact = false;
        private float heightOfJump = 2f;
        private float distanceToGroundForJump = 0.1f;
        public float turnSpeed = 50f;
        private float xRotation = 0f;
        private bool lockRotation;
        private bool jumpLocked;
        private bool duskInProgress = false;
        public bool petitionVoteInProgress = false;
        public GameObject world;
        public Transform playerGround;

        [NonSerialized] public PetitionZone PetitionZone;
        public Dictionary<int, bool> RegisterPetitionVoteByPlayerIDs;

        protected override void SetupClient()
        {
            world = GameObject.FindGameObjectWithTag("World");
            //Disable Cursor
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            lockRotation = false;
            jumpLocked = false;
            turnSpeed = 50f;
            PunPlayer = Client.GetComponent<PunPlayer>();
            PunPlayer.CharacterController.enabled = true;
            WorldManager.minimapCamera.gameObject.SetActive(true);
            WorldManager.minimapCamera.GetComponent<Offset>().Transform = PunClient.Transform;
            playerGround = PunPlayer.transform.GetChild(0).GetChild(2);

            eveningStationType = StationType.None;
            if (WorldManager.mainCamera.TryGetComponent<Offset>(out Offset off))
            {
                off.enabled = false;
            }
            WorldManager.mainCamera.transform.rotation = PunClient.transform.rotation;
            WorldManager.mainCamera.transform.position = PunClient.transform.GetChild(0).GetChild(0).position;
            WorldManager.mainCamera.transform.parent = PunClient.transform.GetChild(0).GetChild(0);

            Station station = WorldManager.GetStationByType(StationType.Home);
            SetStation(station);

            StartCoroutine(CheckHospitalization(LeaveCurrentAndTeleportToTargetStation));
        }

        protected override void UpdateClient()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                PunPlayer.CharacterController.enabled = false;
                TeleportToTargetStation(StationType.Home);
                PunPlayer.CharacterController.enabled = true;
            }
            if (CanInteract && Input.GetKeyDown(ControlSettings.ControlData.CallForHelp))
            {
                ShowCallForHelp();
            }
            HandleLook();
            if (!jumpLocked)
            {
                PunPlayer.CharacterController.Move(HandleGravity() * Time.deltaTime);
            }
            HandleRespawn();

            if (!world.activeSelf || WorldManager.canvasHandler.MenuOpened || WorldManager.state.InteractionInProgress() || duskInProgress || WorldManager.canvasHandler.importantDialogCanvas.transform.childCount > 0)
            {
                EnableCursor();
                Debug.Log("Cursor Enabled - Action Locked");
                lockRotation = true;
                jumpLocked = true;
            }
            else
            {
                if (!WorldManager.state.InteractionInProgress())
                {
                    DisableCursor();
                    lockRotation = false;
                    jumpLocked = false;
                }
            }
        }

        public override void OnUpdated(DayState dayState, DayState oldDayState)
        {
            switch (dayState)
            {
                case DayState.NewDay:
                    foreach (GameObject obj in GameObject.FindGameObjectsWithTag("VoiceController"))
                    {
                        obj.GetComponent<AudioSource>().mute = false;
                    }
                    InteractableConfiguration.ResetCooldowns();

                    WorldManager.canvasHandler.InitializeSimpleDialog(
                        LocalizationUtility.GetLocalizedString("dayEndedTitle"),
                        LocalizationUtility.GetLocalizedString("playerDayEndedMessage"));

                    //GameObject.Find("PunManager").GetComponent<PUN.PunManager>().GameVoiceRegion = VoiceRegion.Global;

                    StartCoroutine(CheckHospitalization(LeaveCurrentAndTeleportToTargetStation));

                    break;

                case DayState.Daylight:

                    foreach (GameObject obj in GameObject.FindGameObjectsWithTag("VoiceController"))
                    {
                        obj.GetComponent<AudioSource>().mute = true;
                    }

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
                                    string message =
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
                            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("VoiceController"))
                            {
                                obj.GetComponent<AudioSource>().mute = false;
                            }
                            break;

                        case -1:
                            StartCoroutine(BackendConnection.GetOpenPetitions(response =>
                            {
                                if (response.IsSuccess())
                                {
                                    PunClient.SendNewEvent(GameEventType.OnStartedDayInHome, GameEventResult.Success);

                                    string message = LocalizationUtility.GetLocalizedString("newDayMessage") + "\n" +
                                                     LocalizationUtility.GetLocalizedString("openPetitionsMessage",
                                                         new[] { response.Data.Count });

                                    /*WorldManager.canvasHandler.InitializeOkDialog(
                                    LocalizationUtility.GetLocalizedString("newDayTitle"), message,
                                            LocalizationUtility.GetLocalizedString("ok"), FeedbackType.Neutral, null);*/
                                }
                                else
                                {
                                    throw new UnityException($"Cannot get open petitions: {response.GetRawResponse()}");
                                }
                            }, true));

                            break;

                        default:
                            throw new UnityException($"Invalid hospitalization state {PunClient.DaysHospitalized}!");
                    }
                    break;

                case DayState.Dusk:
                    if (PunClient.IsHospitalized)
                    {
                        return;
                    }
                    EnableCursor();
                    duskInProgress = true;
                    StartCoroutine(BackendConnection.GetScale(response =>
                    {
                        if (response.IsSuccess())
                        {
                            bool isInLockdown = DeserializationUtility.StringToBool(response.GetRawResponse());


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
                    foreach (GameObject client in GameObject.FindGameObjectsWithTag("VoiceController"))
                    {
                        client.GetComponent<AudioSource>().minDistance = 1f;
                        client.GetComponent<AudioSource>().maxDistance = 30f;
                    }
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
                    duskInProgress = false;
                    DisableCursor();
                    break;
            }
        }

        private IEnumerator GoToPreviouslyChosenEveningStation()
        {
            yield return BackendConnection.GetPlayerProperty(response =>
            {
                if (response.IsSuccess())
                {
                    StationType eveningActivityStationType =
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
            int positiveVotes = 0;
            int negativeVotes = 0;
            foreach (bool registerPetition in RegisterPetitionVoteByPlayerIDs.Values)
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
            if (Input.GetKey(ControlSettings.ControlData.Sprint))
            {
                movement *= gameSettings.PlayerSprintMultiplier;
            }
            PunPlayer.CharacterController.Move(gameSettings.PlayerSpeed * movement * Time.deltaTime);
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
                StationType stationType = StationType.Home;
                if (response.IsSuccess() && response.TryGetResponseAsInt(out int numberDays))
                {
                    PunClient.DaysHospitalized = numberDays switch
                    {
                        2 => 2,
                        1 => 1,
                        0 => PunClient.DaysHospitalized > 0 ? 0 : -1,
                        _ => throw new UnityException($"Invalid number of days in hospital {numberDays}!")
                    };

                    WorldManager.SetPlayerProperties(new Hashtable()
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
        private void HandleLook()
        {
            if (!lockRotation)
            {
                float mouseSpeed = Time.deltaTime * turnSpeed;
                float xMouse = Input.GetAxis("Mouse X") * mouseSpeed;
                float yMouse = Input.GetAxis("Mouse Y") * mouseSpeed;
                xRotation = xRotation - yMouse;
                xRotation = Mathf.Clamp(xRotation, -90f, 90f);
                WorldManager.mainCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
                PunPlayer.Transform.Rotate(Vector3.up * xMouse);
            }
        }

        private Vector3 HandleGravity()
        {
            if (Physics.Raycast(playerGround.position, Vector3.down, distanceToGroundForJump))
            {
                groundContact = true;
            }
            else
            {
                groundContact = false;
            }
            if (groundContact && fallVector.y < 0)
            {
                fallVector.y = -2f;
            }
            //if (Input.GetButtonDown("Jump") && groundContact)
            if (Input.GetKeyDown(ControlSettings.ControlData.Jump) && groundContact)
            {
                fallVector.y = Mathf.Sqrt(heightOfJump * -2f * gravity);
            }
            //ControlSettings.ControlData.PrimaryForward
            fallVector.y = fallVector.y + Time.deltaTime * gravity * 1.2f;
            return fallVector;
        }

        public void EnableCursor()
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            lockRotation = true;
        }

        public void DisableCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            lockRotation = false;
        }

        private void OnDestroy()
        {
            EnableCursor();
        }
    }
}