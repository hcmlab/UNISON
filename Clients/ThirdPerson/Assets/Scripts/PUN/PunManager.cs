using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.Unity;
using Sentry;
using UI;
using UI.Dialogs;
using UnityEngine;
using World;

namespace PUN
{
    public enum LeaveReason
    {
        ByMyself,
        GameFinished,
        MasterClientLeft,
        Disconnected
    }

    public class PunManager : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        public const byte StartGameEventCode = 1;
        public const byte FinishGameEventCode = 3;

        public static readonly RoomOptions DefaultRoomOptions = new RoomOptions
        {
            EmptyRoomTtl = 0,
            PublishUserId = true
        };

        public static readonly Hashtable DefaultRoomProperties = new Hashtable
        {
            { "gameRound", 1 },
            { "dayState", DayState.Daylight },
            { "daytimeProgress", 0.0f },
            { "gameStarted", false },
            { "petitionUser", -1 }
        };

        [SerializeField] private GameObject voiceNetwork;
        [SerializeField] private GameObject voiceClientPrefab;
        public Camera mainCamera;

        public GameObject world;
        public CanvasHandler canvasHandler;
        private List<IStateSubscriber<VoiceRegion>> gameVoiceRegionSubscribers;
        private GameObject localVoiceClient;

        private Dictionary<Player, VoiceClient> voiceClients;

        public bool GameStarted { get; private set; }
        public VoiceRegion WorldVoiceRegion { get; private set; }
        public VoiceRegion GameVoiceRegion => GameStarted ? WorldVoiceRegion : VoiceRegion.Global;

        private void Awake()
        {
            voiceClients = new Dictionary<Player, VoiceClient>();
            gameVoiceRegionSubscribers = new List<IStateSubscriber<VoiceRegion>>();
        }

        private void Start()
        {
            PhotonNetwork.AddCallbackTarget(this);

            if (!PhotonNetwork.IsConnected)
            {
                PhotonNetwork.ConnectUsingSettings();
            }
        }

        private void Update()
        {
            voiceNetwork.GetComponent<Recorder>().enabled = Microphone.devices.Any();
        }

        public void OnEvent(EventData eventData)
        {
            switch (eventData.Code)
            {
                case StartGameEventCode:
                    StartGame();
                    break;
                case FinishGameEventCode:
                    LeaveGame(LeaveReason.GameFinished);
                    break;
            }
        }

        public void SetPlayerName(string playerName)
        {
            PlayerPrefs.SetString("PlayerName", playerName);
            PhotonNetwork.NickName = playerName;
        }

        public bool HostGame(string roomName, RoomOptions options, string[] expectedUsers = null)
        {
            if (PhotonNetwork.IsConnectedAndReady)
            {
                if (PhotonNetwork.CreateRoom(roomName, options, null, expectedUsers))
                {
                    return true;
                }
            }

            return false;
        }

        public override void OnJoinedRoom()
        {
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("gameStarted", out var started) &&
                (bool)started)
            {
                StartGame();
            }
            else if (localVoiceClient == null)
            {
                localVoiceClient = PhotonNetwork.Instantiate(voiceClientPrefab.name, Vector3.zero, Quaternion.identity);
            }
        }

        public void UpdateWorldVoiceRegion(VoiceRegion worldVoiceRegion)
        {
            if (WorldVoiceRegion == worldVoiceRegion)
            {
                return;
            }

            var oldGameVoiceRegion = GameVoiceRegion;
            WorldVoiceRegion = worldVoiceRegion;
            if (GameVoiceRegion == oldGameVoiceRegion)
            {
                return;
            }

            foreach (var subscriber in gameVoiceRegionSubscribers)
            {
                subscriber.OnUpdated(GameVoiceRegion, oldGameVoiceRegion);
            }
        }

        public void SubscribeToGameVoiceRegion(IStateSubscriber<VoiceRegion> subscriber, bool updateInitial = false)
        {
            if (gameVoiceRegionSubscribers.Contains(subscriber))
            {
                Debug.LogWarning("Tried to register a voice region subscriber which has already been registered!");
                return;
            }

            gameVoiceRegionSubscribers.Add(subscriber);
            if (updateInitial)
            {
                subscriber.OnUpdated(GameVoiceRegion, GameVoiceRegion);
            }
        }

        public void UnsubscribeFromGameVoiceRegion(IStateSubscriber<VoiceRegion> subscriber)
        {
            if (gameVoiceRegionSubscribers.Contains(subscriber))
            {
                gameVoiceRegionSubscribers.Remove(subscriber);
            }
        }

        public void RegisterVoiceClient(VoiceClient voiceClient)
        {
            if (voiceClients.TryGetValue(voiceClient.photonView.Owner, out _))
            {
                Debug.LogWarning(
                    $"Tried to register a voice client for player {voiceClient.photonView.Owner.NickName} when there already has been one registered!");
                return;
            }

            voiceClients.Add(voiceClient.photonView.Owner, voiceClient);
        }

        public void UnregisterVoiceClient(VoiceClient voiceClient)
        {
            if (voiceClients.TryGetValue(voiceClient.photonView.Owner, out _))
            {
                voiceClients.Remove(voiceClient.photonView.Owner);
            }
        }

        public bool TryGetVoiceClient(Player player, out VoiceClient voiceClient)
        {
            return voiceClients.TryGetValue(player, out voiceClient);
        }

        private void SetGameStarted(bool gameStarted)
        {
            if (GameStarted == gameStarted)
            {
                return;
            }

            var oldGameVoiceRegion = GameVoiceRegion;
            GameStarted = gameStarted;
            if (GameVoiceRegion == oldGameVoiceRegion)
            {
                return;
            }

            foreach (var subscriber in gameVoiceRegionSubscribers)
            {
                subscriber.OnUpdated(GameVoiceRegion, oldGameVoiceRegion);
            }
        }

        private void StartGame()
        {
            if (localVoiceClient)
            {
                PhotonNetwork.Destroy(localVoiceClient);
            }

            voiceClients.Clear();
            SetGameStarted(true);
            world.SetActive(true);
        }

        public void LeaveGame(LeaveReason reason = LeaveReason.ByMyself)
        {
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.LeaveRoom();
            }

            if (localVoiceClient)
            {
                PhotonNetwork.Destroy(localVoiceClient);
            }

            voiceClients.Clear();
            world.SetActive(false);
            SetGameStarted(false);
            HandleLeaveReason(reason);

            SentrySdk.ConfigureScope(scope => { scope.User = null!; });
        }

        public override void OnLeftRoom()
        {
            LeaveGame();
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            LeaveGame(LeaveReason.MasterClientLeft);
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            LeaveGame(LeaveReason.Disconnected);
        }

        private void HandleLeaveReason(LeaveReason reason)
        {
            switch (reason)
            {
                case LeaveReason.GameFinished:
                    canvasHandler.InitializeOkDialog(LocalizationUtility.GetLocalizedString("congratulationsTitle"),
                        LocalizationUtility.GetLocalizedString("congratulationsMessage"),
                        LocalizationUtility.GetLocalizedString("ok"), FeedbackType.Positive, null);
                    break;

                case LeaveReason.MasterClientLeft:
                    canvasHandler.InitializeOkDialog(LocalizationUtility.GetLocalizedString("lobbyClosedTitle"),
                        LocalizationUtility.GetLocalizedString("lobbyClosedMessage"),
                        LocalizationUtility.GetLocalizedString("ok"), FeedbackType.Negative, null);
                    break;

                case LeaveReason.Disconnected:
                    canvasHandler.InitializeOkDialog(LocalizationUtility.GetLocalizedString("disconnectedTitle"),
                        LocalizationUtility.GetLocalizedString("disconnectedMessage"),
                        LocalizationUtility.GetLocalizedString("ok"), FeedbackType.Negative, null);
                    break;
            }
        }
    }
}