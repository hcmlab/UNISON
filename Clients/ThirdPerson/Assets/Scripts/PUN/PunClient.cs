using System;
using ExitGames.Client.Photon;
using JetBrains.Annotations;
using Photon.Pun;
using Photon.Realtime;
using Stations;
using UnityEngine;
using World;

namespace PUN
{
    public abstract class PunClient : MonoBehaviourPun
    {
        public GameObject controllerObject;
        public VoiceClient voiceClient;

        // state
        private Station currentStation;

        [NonSerialized] public bool IsMoving;

        // references
        protected WorldManager CurrentWorldManager { get; private set; }

        public Transform Transform { get; private set; }

        public Station CurrentStation
        {
            get => !currentStation ? CurrentWorldManager.GetStationByType(StationType.None) : currentStation;
            set => currentStation = value;
        }

        public StationType CurrentStationType => !currentStation ? StationType.None : currentStation.Type;
        public int DaysHospitalized { get; set; } = -1;

        // getters
        public Player Player => photonView.Owner;
        public int ID => Player.ActorNumber;
        public string Name => Player.NickName;
        public bool IsMine => photonView.IsMine;
        public bool IsHospitalized => DaysHospitalized > 0;

        private void Awake()
        {
            CurrentWorldManager = GameObject.FindWithTag("World").GetComponent<WorldManager>();
            Transform = controllerObject.transform;

            AwakeClient();
        }

        private void Start()
        {
            CurrentWorldManager.RegisterEvent(new GameEvent(photonView.Owner, GameEventType.OnJoinedGame,
                GameEventResult.Success));
            UpdateCustomProperties(photonView.Owner.CustomProperties, true);

            StartClient();
        }

        private void Update()
        {
            UpdateClient();
        }

        private void OnDestroy()
        {
            if (CurrentWorldManager)
            {
                CurrentWorldManager.RegisterEvent(new GameEvent(photonView.Owner, GameEventType.OnLeftGame,
                    GameEventResult.Success));
            }

            OnDestroyClient();
        }

        protected virtual void AwakeClient()
        {
        }

        protected virtual void StartClient()
        {
        }

        protected virtual void UpdateClient()
        {
        }

        protected virtual void OnDestroyClient()
        {
        }

        public void RPC(string methodName, RpcTarget target, params object[] parameters)
        {
            photonView.RPC(methodName, target, parameters);
        }

        public void UpdateCustomProperties(Hashtable properties, bool initial = false)
        {
            if (!IsMine && properties.TryGetValue("stationType", out var stationType))
            {
                OnStationChanged((StationType)stationType);
            }

            if (properties.TryGetValue("daysHospitalized", out var daysHospitalized))
            {
                DaysHospitalized = (int)daysHospitalized;
            }

            UpdateCustomPropertiesClient(properties, initial);
        }

        protected virtual void UpdateCustomPropertiesClient(Hashtable properties, bool initial = false)
        {
        }

        [PunRPC]
        [UsedImplicitly]
        public void OnStationChanged(StationType stationType)
        {
            CurrentStation = CurrentWorldManager.GetStationByType(stationType);
        }

        public void SendNewEvent(GameEventType gameEventType, GameEventResult gameEventResult,
            params object[] parameters)
        {
            SendEvent(new GameEvent(photonView.Owner, gameEventType, gameEventResult, parameters));
        }

        private void SendEvent(GameEvent gameEvent)
        {
            RPC("OnEventReceived", RpcTarget.MasterClient, gameEvent.GetData(), 0);
        }

        [PunRPC]
        [UsedImplicitly]
        public void OnEventReceived(object[] data, int zero, PhotonMessageInfo info)
        {
            var gEvent = new GameEvent(info.Sender, data);
            CurrentWorldManager.RegisterEvent(gEvent);
        }
    }
}