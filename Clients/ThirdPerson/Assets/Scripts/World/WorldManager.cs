using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Backend;
using Clients;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using PUN;
using Settings;
using Stations;
using Stations.Zones;
using UI;
using UI.Dialogs;
using UI.HUD;
using UI.Menu;
using UnityEngine;
using UnityEngine.Audio;
using AudioSettings = Settings.AudioSettings;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace World
{
    public class WorldManager : MonoBehaviour, IInRoomCallbacks, IOnEventCallback, IStateSubscriber<Station>,
        IStateSubscriber<VoiceRegion>
    {
        // constants
        public const byte UpdateDayStateCode = 2;

        private const int CompareAndSwapRoomPropertyTimeout = 5;

        // references
        [SerializeField] private VoiceActivityTracker voiceActivityTracker;
        [SerializeField] private PunManager punManager;
        [SerializeField] private GameSettings gameSettings;

        public CanvasHandler canvasHandler;

        [SerializeField] private AudioMixer audioMixer;
        public GameObject focusObject;
        public Camera mainCamera;
        public Camera minimapCamera;

        public Station[] stations;

        [SerializeField] private GameObject[] trophyParts;

        [SerializeField] private GameObject[] syringeObjects;

        // parameters & prefabs
        [SerializeField] private GameObject masterManagerPrefab;
        [SerializeField] private GameObject playerManagerPrefab;
        [SerializeField] private GameObject voiceStateHUDPrefab;

        [SerializeField] private float syncDaytimeProgressInterval = 2f;

        // day state
        private int daylightDuration = 480;

        private List<IStateSubscriber<DayState>> dayStateSubscribers;
        private int duskDuration = 15;

        private List<GameEvent> eventFeed;
        private List<ICollectionSubscriber<GameEvent>> eventSubscribers;
        private bool hospitalAmbienceEnabled;

        private OkDialog okDialog;
        private bool otherPlayersAudible;

        private bool otherPlayersVisible;
        private Dictionary<Player, PunPlayer> players;
        private List<ICollectionSubscriber<PunPlayer>> playerSubscribers;
        private WorldSoundManager soundManager;
        private Dictionary<StationType, Station> stationDict;

        private bool syringesEnabled;

        private GameObject voiceStateHUD;

        public PlayerManager PlayerManager { get; private set; }
        public MasterManager MasterManager { get; private set; }
        public ClientManager ClientManager { get; private set; }

        // world state
        public PunMaster Master { get; private set; }

        public float RegisterPetitionDialogDuration => gameSettings.RegisterPetitionDialogDuration;

        public float DaytimeUpdateTime { get; private set; }
        public int GameRound { get; private set; } = 1;
        public DayState DayState { get; private set; }
        public float DaytimeProgress { get; private set; }

        public float RemainingDaytime => DaytimeUpdateTime - Time.time;

        public bool NextDayFaded { get; private set; }

        // getters
        public static StationType DefaultEveningStationType => StationType.TownHall;
        public static string RoomName => PhotonNetwork.CurrentRoom.Name;
        public static bool IsMasterClient => PhotonNetwork.IsMasterClient;
        private static Hashtable RoomProperties => PhotonNetwork.CurrentRoom.CustomProperties;

        private void Awake()
        {
            soundManager = GetComponent<WorldSoundManager>();

            players = new Dictionary<Player, PunPlayer>();
            playerSubscribers = new List<ICollectionSubscriber<PunPlayer>>();
            dayStateSubscribers = new List<IStateSubscriber<DayState>>();
            eventSubscribers = new List<ICollectionSubscriber<GameEvent>>();
            eventFeed = new List<GameEvent>();
        }

        private void Update()
        {
            UpdateDaytimeProgress();

            UpdateTriggerElements();

            if (Input.GetKeyDown(ControlSettings.ControlData.ToggleMenu))
            {
                canvasHandler.ToggleMenu(!canvasHandler.MenuOpened);
            }
        }

        private void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);

            canvasHandler.TransitionCanvas.FadeOutImmediately(TransitionType.State);

            // initiate client
            if (IsMasterClient)
            {
                MasterManager = InstantiateMaster();
                ClientManager = MasterManager;
            }
            else
            {
                PlayerManager = InstantiatePlayer();
                ClientManager = PlayerManager;
            }

            ClientManager.WorldManager = this;
            ClientManager.SubscribeToStation(this, true);
            ClientManager.SubscribeToStation(GetComponent<AmbienceSoundManager>(), true);
            ClientManager.Setup();

            canvasHandler.HUDManager.Setup(this, IsMasterClient);
            canvasHandler.ToggleMenu(false);

            punManager.SubscribeToGameVoiceRegion(this, true);

            if (!IsMasterClient)
            {
                voiceActivityTracker.Setup(PlayerManager);
                PlayerManager.PunPlayer.voiceClient.SubscribeToVoiceState(voiceActivityTracker, true);
            }

            // prepare state
            UpdateTriggerElements(true);

            DayState = DayState.GameStarted;
            daylightDuration = (int)RoomProperties["daylightDuration"];
            duskDuration = gameSettings.EveningDialogDuration;

            InteractableConfiguration.CooldownInSecondsByZones[ZoneType.Relax] =
                (float)RoomProperties["relaxCooldownInSeconds"];

            var gameRound = (int)RoomProperties["gameRound"];
            var dayState = (DayState)RoomProperties["dayState"];
            var daytimeProgress = (float)RoomProperties["daytimeProgress"];

            OnDayStateUpdate(gameRound, dayState, daytimeProgress);
            if (IsMasterClient)
            {
                otherPlayersVisible = true;
                otherPlayersAudible = true;
                StartCoroutine(SyncDaytimeProgress());
            }

            StartCoroutine(UpdateTrophy());
        }

        private void OnDisable()
        {
            ClientManager.UnsubscribeFromStation(this);
            ClientManager.UnsubscribeFromStation(GetComponent<AmbienceSoundManager>());

            punManager.UnsubscribeFromGameVoiceRegion(this);

            if (!IsMasterClient)
            {
                PlayerManager.PunPlayer.voiceClient.UnsubscribeFromVoiceState(voiceActivityTracker);
            }

            Destroy(voiceStateHUD);
            Destroy(ClientManager.gameObject);

            players.Clear();
            playerSubscribers.Clear();
            dayStateSubscribers.Clear();
            eventSubscribers.Clear();
            eventFeed.Clear();

            canvasHandler.HUDManager.Clear();
            canvasHandler.ClearDialogCanvas();
            canvasHandler.ToggleMenu(true);

            PhotonNetwork.RemoveCallbackTarget(this);
        }

        public void OnPlayerEnteredRoom(Player newPlayer)
        {
        }

        public void OnPlayerLeftRoom(Player otherPlayer)
        {
        }

        public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
        }

        public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            if (targetPlayer.IsMasterClient)
            {
                if (Master)
                {
                    Master.UpdateCustomProperties(changedProps);
                }
            }
            else if (players.TryGetValue(targetPlayer, out var punPlayer))
            {
                punPlayer.UpdateCustomProperties(changedProps);
            }
        }

        public void OnMasterClientSwitched(Player newMasterClient)
        {
        }

        public void OnEvent(EventData eventData)
        {
            switch (eventData.Code)
            {
                case UpdateDayStateCode:
                    var data = (object[])eventData.CustomData;
                    OnDayStateUpdate((int)data[0], (DayState)data[1], (float)data[2]);
                    break;
            }
        }

        public void OnUpdated(Station station, Station oldStation)
        {
            if (IsMasterClient)
            {
                return;
            }

            var visible = !station.HideOtherPlayers;
            UpdatePlayerPerceptibility(visible, visible || otherPlayersAudible);
        }

        public void OnUpdated(VoiceRegion voiceRegion, VoiceRegion oldVoiceRegion)
        {
            if (IsMasterClient)
            {
                return;
            }

            UpdatePlayerPerceptibility(otherPlayersVisible, otherPlayersVisible || voiceRegion == VoiceRegion.Global);
        }

        private MasterManager InstantiateMaster()
        {
            var obj = Instantiate(masterManagerPrefab, Vector3.zero, Quaternion.identity);
            var manager = obj.GetComponent<MasterManager>();

            return manager;
        }

        private PlayerManager InstantiatePlayer()
        {
            var obj = Instantiate(playerManagerPrefab, Vector3.zero, Quaternion.identity);
            var manager = obj.GetComponent<PlayerManager>();

            return manager;
        }

        private void UpdateDaytimeProgress()
        {
            var daytimeDuration = GetDaytimeDuration(DayState);
            DaytimeProgress = 1 - RemainingDaytime / daytimeDuration;
        }

        private IEnumerator SyncDaytimeProgress()
        {
            while (isActiveAndEnabled)
            {
                SetRoomProperties(new Hashtable
                {
                    { "daytimeProgress", DaytimeProgress }
                });

                yield return new WaitForSeconds(syncDaytimeProgressInterval);
            }
        }

        private void UpdateTriggerElements(bool force = false)
        {
            if (force || syringesEnabled != SettingsMenu.SyringesEnabled)
            {
                syringesEnabled = SettingsMenu.SyringesEnabled;
                foreach (var obj in syringeObjects)
                {
                    obj.SetActive(syringesEnabled);
                }
            }

            if (force || hospitalAmbienceEnabled != SettingsMenu.HospitalSoundEnabled)
            {
                hospitalAmbienceEnabled = SettingsMenu.HospitalSoundEnabled;
                GetStationByType(StationType.Hospital).IsAmbienceEnabled = hospitalAmbienceEnabled;
            }
        }

        private void UpdatePlayerPerceptibility(bool visible, bool audible)
        {
            if (otherPlayersVisible != visible)
            {
                otherPlayersVisible = visible;
                foreach (var player in players.Values.ToList())
                {
                    if (player.ID == ClientManager.PunClient.ID)
                    {
                        continue;
                    }

                    player.avatarObject.SetActive(visible);
                }
            }

            if (otherPlayersAudible != audible)
            {
                otherPlayersAudible = audible;
                audioMixer.SetFloat("PlayerVolume", AudioSettings.ScaleVolume(audible ? 1f : 0f));
            }
        }

        public void SubscribeToPlayers(ICollectionSubscriber<PunPlayer> subscriber, bool addInitial = false)
        {
            if (playerSubscribers.Contains(subscriber))
            {
                Debug.LogError("Tried to register a player subscriber which has already been registered!");
                return;
            }

            playerSubscribers.Add(subscriber);
            if (!addInitial)
            {
                return;
            }

            foreach (var client in players.Values.ToList())
            {
                subscriber.OnAdded(client);
            }
        }

        public void UnsubscribeFromPlayers(ICollectionSubscriber<PunPlayer> subscriber)
        {
            if (playerSubscribers.Contains(subscriber))
            {
                playerSubscribers.Remove(subscriber);
            }
        }

        public void SubscribeToDayState(IStateSubscriber<DayState> subscriber, bool updateInitial = false)
        {
            if (dayStateSubscribers.Contains(subscriber))
            {
                Debug.LogError("Tried to register a day state subscriber which has already been registered!");
                return;
            }

            dayStateSubscribers.Add(subscriber);
            if (updateInitial)
            {
                subscriber.OnUpdated(DayState, DayState);
            }
        }

        public void UnsubscribeFromDayState(IStateSubscriber<DayState> subscriber)
        {
            if (dayStateSubscribers.Contains(subscriber))
            {
                dayStateSubscribers.Remove(subscriber);
            }
        }

        public void RegisterEvent(GameEvent gameEvent)
        {
            eventFeed.Add(gameEvent);
            foreach (var subscriber in eventSubscribers)
            {
                subscriber.OnAdded(gameEvent);
            }
        }

        public void SubscribeToEvents(ICollectionSubscriber<GameEvent> subscriber, bool addInitial = false)
        {
            if (eventSubscribers.Contains(subscriber))
            {
                Debug.LogError("Tried to register an event subscriber which has already been registered!");
                return;
            }

            eventSubscribers.Add(subscriber);
            if (!addInitial)
            {
                return;
            }

            foreach (var gameEvent in eventFeed)
            {
                subscriber.OnAdded(gameEvent);
            }
        }

        public void UnsubscribeFromEvents(ICollectionSubscriber<GameEvent> subscriber)
        {
            if (eventSubscribers.Contains(subscriber))
            {
                eventSubscribers.Remove(subscriber);
            }
        }

        public void RegisterMaster(PunMaster punMaster)
        {
            if (Master)
            {
                Debug.LogError("Tried to register the PUN master client when there already has been one registered!");
                return;
            }

            Master = punMaster;
        }

        public void UnregisterMaster(PunMaster punMaster)
        {
            if (Master.ID != punMaster.ID)
            {
                Debug.LogError(
                    $"Tried to unregister the PUN master client {punMaster.Name} when they are not the registered PUN master client!");
                return;
            }

            Master = null;
        }

        public void RegisterPlayer(PunPlayer player)
        {
            if (players.TryGetValue(player.photonView.Owner, out _))
            {
                Debug.LogError(
                    $"Tried to register a PUN client for player {player.Name} when there already has been one registered!");
                return;
            }

            if (!IsMasterClient && player.ID != ClientManager.PunClient.ID)
            {
                if (!otherPlayersVisible)
                {
                    player.avatarObject.SetActive(false);
                }
            }

            players.Add(player.photonView.Owner, player);
            foreach (var subscriber in playerSubscribers)
            {
                subscriber.OnAdded(player);
            }
        }

        public void UnregisterPlayer(PunPlayer player)
        {
            if (players.TryGetValue(player.photonView.Owner, out _))
            {
                players.Remove(player.photonView.Owner);
                foreach (var subscriber in playerSubscribers)
                {
                    subscriber.OnRemoved(player);
                }

                if (IsMasterClient)
                {
                    SetRoomProperties(new Hashtable { { "petitionUser", -1 } },
                        new Hashtable { { "petitionUser", player.ID } });
                }
            }
        }

        public bool TryGetPlayer(Player player, out PunPlayer punPlayer)
        {
            return players.TryGetValue(player, out punPlayer);
        }

        public bool TryGetPlayerByActorNumber(int actorNumber, out PunPlayer punPlayer)
        {
            var player = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);
            if (player == null)
            {
                punPlayer = null;
                return false;
            }

            return players.TryGetValue(player, out punPlayer);
        }

        public bool SetRoomProperties(Hashtable propertiesToSet, Hashtable expectedProperties = null)
        {
            return PhotonNetwork.CurrentRoom.SetCustomProperties(propertiesToSet, expectedProperties);
        }

        public bool SetPlayerProperties(Hashtable propertiesToSet, Hashtable expectedProperties = null)
        {
            return PhotonNetwork.LocalPlayer.SetCustomProperties(propertiesToSet, expectedProperties);
        }

        public IEnumerator CompareAndSwapRoomProperty<T>(string key, T compare, T swap,
            Action<T> onSuccess = null, Action<T> onFail = null, Action onFinally = null, Action onTimeout = null)
        {
            if (!RoomProperties.TryGetValue(key, out var obj))
            {
                SetRoomProperties(new Hashtable { { key, swap } });
                onSuccess?.Invoke(swap);
                onFinally?.Invoke();
                yield break;
            }

            if (!(obj is T value))
            {
                throw new UnityException($"Room property {key} is of type {obj.GetType()}, not {typeof(T)}!");
            }

            bool Check()
            {
                if (value.Equals(swap))
                {
                    onSuccess?.Invoke(value);
                    onFinally?.Invoke();
                    return true;
                }

                if (!value.Equals(compare))
                {
                    onFail?.Invoke(value);
                    onFinally?.Invoke();
                    return true;
                }

                return false;
            }

            if (Check())
            {
                yield break;
            }

            if (!SetRoomProperties(new Hashtable { { key, swap } }, new Hashtable { { key, compare } }))
            {
                onFail?.Invoke(value);
                onFinally?.Invoke();
                yield break;
            }

            var timeout = Time.time + CompareAndSwapRoomPropertyTimeout;
            while (timeout > Time.time)
            {
                value = (T)RoomProperties[key];
                if (Check())
                {
                    yield break;
                }

                yield return new WaitForSeconds(0.05f);
            }

            onFail?.Invoke(value);
            onFinally?.Invoke();
            onTimeout?.Invoke();
        }

        public IEnumerator CompareAndSwapRoomProperty<T>(string key, T compare, T swap, T fallback,
            Action<T> onSuccess = null, Action<T> onFail = null, Action onFinally = null, Action onTimeout = null)
        {
            void OnTimeoutWithFallback()
            {
                if (fallback != null)
                {
                    SetRoomProperties(new Hashtable { { key, fallback } });
                }

                onTimeout?.Invoke();
            }

            return CompareAndSwapRoomProperty(key, compare, swap, onSuccess, onFail, onFinally, OnTimeoutWithFallback);
        }

        public Station GetStationByType(StationType stationType)
        {
            if (stationDict == null)
            {
                stationDict = new Dictionary<StationType, Station>();
                foreach (var s in stations)
                {
                    stationDict[s.Type] = s;
                }
            }

            if (stationDict.TryGetValue(stationType, out var station))
            {
                return station;
            }

            throw new UnityException($"Unknown station {stationType}!");
        }

        private void OnDayStateUpdate(int gameRound, DayState dayState, float daytimeProgress)
        {
            if (DayState == dayState)
            {
                return;
            }

            var oldDayState = DayState;

            GameRound = gameRound;
            DayState = dayState;
            var daytimeDuration = GetDaytimeDuration(dayState);
            DaytimeUpdateTime = Time.time + daytimeDuration * (1 - daytimeProgress);

            // leave day state
            canvasHandler.ClearDialogCanvas();
            canvasHandler.TransitionCanvas.FadeIn(TransitionType.State);
            punManager.UpdateWorldVoiceRegion(VoiceRegion.Local);
            switch (oldDayState)
            {
                case DayState.GameStarted:
                    break;
                case DayState.NextDay:
                    NextDayFaded = false;
                    break;
                case DayState.NewDay:
                    Destroy(voiceStateHUD);
                    break;
                case DayState.Daylight:
                    break;
                case DayState.Dusk:
                    break;
                case DayState.Evening:
                    break;
                case DayState.GameEnded:
                    return;
            }

            // enter day state
            switch (dayState)
            {
                case DayState.NextDay:
                    canvasHandler.TransitionCanvas.FadeOut(TransitionType.State, () => NextDayFaded = true);
                    canvasHandler.InitializeSimpleDialog(LocalizationUtility.GetLocalizedString("nextDayTitle"),
                        LocalizationUtility.GetLocalizedString("nextDayMessage"));
                    break;

                case DayState.NewDay:
                    canvasHandler.TransitionCanvas.FadeOut(TransitionType.State);
                    punManager.UpdateWorldVoiceRegion(VoiceRegion.Global);
                    InstantiateVoiceStateHUD();
                    break;

                case DayState.Daylight:
                    if (!ClientManager.PunClient.IsHospitalized)
                    {
                        soundManager.Play("Daylight");
                    }

                    RegisterEvent(new GameEvent(null, GameEventType.OnDaylight, GameEventResult.Success, gameRound));
                    break;

                case DayState.Dusk:
                    if (!ClientManager.PunClient.IsHospitalized)
                    {
                        soundManager.Play("Evening");
                    }

                    RegisterEvent(new GameEvent(null, GameEventType.OnEvening, GameEventResult.Success, gameRound));
                    break;

                case DayState.Evening:
                    break;

                case DayState.GameEnded:
                    canvasHandler.TransitionCanvas.FadeOut(TransitionType.State);
                    punManager.UpdateWorldVoiceRegion(VoiceRegion.Global);
                    InstantiateVoiceStateHUD();

                    okDialog = canvasHandler.InitializeOkDialog(
                        LocalizationUtility.GetLocalizedString("gameEndedTitle"),
                        LocalizationUtility.GetLocalizedString("gameEndedMessage"),
                        LocalizationUtility.GetLocalizedString("ok"), FeedbackType.Positive, () =>
                        {
                            punManager.UpdateWorldVoiceRegion(VoiceRegion.Local);
                            Destroy(voiceStateHUD);
                            Destroy(okDialog.gameObject);
                            punManager.LeaveGame(LeaveReason.GameFinished);
                        });
                    break;
            }

            foreach (var subscriber in dayStateSubscribers)
            {
                subscriber.OnUpdated(DayState, oldDayState);
            }
        }

        private void InstantiateVoiceStateHUD()
        {
            voiceStateHUD = Instantiate(voiceStateHUDPrefab, canvasHandler.transform);
            voiceStateHUD.GetComponent<VoiceStateHUD>().OnBeingEnabled(this);
        }

        private float GetDaytimeDuration(DayState dayState)
        {
            return dayState switch
            {
                DayState.Dusk => duskDuration,
                DayState.Daylight => daylightDuration,
                _ => 0
            };
        }

        public void Save(string clientName)
        {
            LoadSave.AddOrUpdateMasterSaveGame(new MasterSaveGame(BackendConnection.InstanceName, clientName,
                PhotonNetwork.CurrentRoom.MaxPlayers, daylightDuration,
                InteractableConfiguration.GetCooldownInSecondsForZone(ZoneType.Relax), GameRound, DayState,
                DaytimeProgress));
        }

        public (bool, string) IsInteractableActive(StationType station, ZoneType zone)
        {
            switch (zone)
            {
                case ZoneType.BuyCarePackage:
                case ZoneType.HelpDesk:
                case ZoneType.GetPetitions:
                case ZoneType.GetGlobalFunds:
                case ZoneType.GetGraphs:
                    return (true, LocalizationUtility.GetLocalizedString("interactableActiveWarning"));

                case ZoneType.Buy:
                    return (DayState == DayState.Daylight,
                        LocalizationUtility.GetLocalizedString("buyInteractableActive"));
                case ZoneType.Invest:
                    return (DayState == DayState.Daylight,
                        LocalizationUtility.GetLocalizedString("investInteractableActive"));
                case ZoneType.SendMoney:
                    return (DayState == DayState.Daylight,
                        LocalizationUtility.GetLocalizedString("sendMoneyInteractableActive"));
                case ZoneType.EarnMoney:
                    return (DayState == DayState.Daylight,
                        LocalizationUtility.GetLocalizedString("earnMoneyInteractableActive"));
                case ZoneType.Learn:
                    return (DayState == DayState.Daylight,
                        LocalizationUtility.GetLocalizedString("learnInteractableActive"));
                case ZoneType.Vote:
                    return (DayState == DayState.Daylight,
                        LocalizationUtility.GetLocalizedString("voteInteractableActive"));

                case ZoneType.Relax:
                    return (DayState == DayState.Evening,
                        LocalizationUtility.GetLocalizedString("relaxInteractableActive"));
                case ZoneType.Petition:
                    return (DayState == DayState.Evening,
                        LocalizationUtility.GetLocalizedString("petitionInteractableActive"));

                case ZoneType.Entry:
                    switch (station)
                    {
                        case StationType.Home:
                        case StationType.MarketSquare:
                            return (true, LocalizationUtility.GetLocalizedString("interactableActiveWarning"));

                        case StationType.TownHall:
                            return (DayState == DayState.Daylight,
                                LocalizationUtility.GetLocalizedString("enterTownHallInteractableActive"));
                        case StationType.Mall:
                            return (DayState == DayState.Daylight,
                                LocalizationUtility.GetLocalizedString("enterMallInteractableActive"));
                        case StationType.School:
                            return (DayState == DayState.Daylight,
                                LocalizationUtility.GetLocalizedString("enterSchoolInteractableActive"));
                        case StationType.Office:
                            return (DayState == DayState.Daylight,
                                LocalizationUtility.GetLocalizedString("enterOfficeInteractableActive"));

                        case StationType.Lounge:
                            return (DayState == DayState.Evening,
                                LocalizationUtility.GetLocalizedString("enterLoungeInteractableActive"));

                        case StationType.Hospital:
                            return (false, LocalizationUtility.GetLocalizedString("enterHospitalInteractableActive"));

                        default:
                            throw new UnityException($"Unknown station type {station}!");
                    }

                case ZoneType.Exit:
                    switch (station)
                    {
                        case StationType.Home:
                        case StationType.MarketSquare:
                        case StationType.Mall:
                        case StationType.School:
                        case StationType.Office:
                        case StationType.Lounge:
                            return (true, LocalizationUtility.GetLocalizedString("interactableActiveWarning"));

                        case StationType.TownHall:
                            return (DayState == DayState.Daylight,
                                LocalizationUtility.GetLocalizedString("exitTownHallInteractableActive"));

                        case StationType.Hospital:
                            return (false, LocalizationUtility.GetLocalizedString("exitHospitalInteractableActive"));

                        default:
                            throw new UnityException($"Unknown station type {station}!");
                    }

                default:
                    throw new UnityException($"Unknown zone type {zone}!");
            }
        }

        private IEnumerator UpdateTrophy()
        {
            while (isActiveAndEnabled)
            {
                yield return BackendConnection.GetPetitions(response =>
                {
                    if (response.IsSuccess())
                    {
                        var numberClosedPetitions = 0;
                        foreach (var petition in response.Data)
                        {
                            if (petition.Status == "closed")
                            {
                                ++numberClosedPetitions;
                            }
                        }

                        // Base of the trophy.
                        trophyParts[0].SetActive(numberClosedPetitions > 0);
                        trophyParts[1].SetActive(numberClosedPetitions > 1);
                        trophyParts[2].SetActive(numberClosedPetitions > 2);

                        // Cylinder of the trophy.
                        trophyParts[3].SetActive(numberClosedPetitions > 3);
                        trophyParts[4].SetActive(numberClosedPetitions > 4);
                        trophyParts[5].SetActive(numberClosedPetitions > 5);
                        trophyParts[6].SetActive(numberClosedPetitions > 6);

                        // Star of the trophy.
                        trophyParts[7].SetActive(numberClosedPetitions > 7);
                        trophyParts[8].SetActive(numberClosedPetitions > 8);
                        trophyParts[9].SetActive(numberClosedPetitions > 8);
                        trophyParts[10].SetActive(numberClosedPetitions > 9);
                        trophyParts[11].SetActive(numberClosedPetitions > 10);
                        trophyParts[12].SetActive(numberClosedPetitions > 11);
                    }
                });
                yield return new WaitForSeconds(3f);
            }
        }
    }
}