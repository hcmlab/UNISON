using System;
using JetBrains.Annotations;
using Photon.Pun;
using Photon.Realtime;
using Stations;
using UnityEngine;
using World;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace PUN
{
    public abstract class PunClient : MonoBehaviourPun
    {
        // references
        protected WorldManager CurrentWorldManager { get; private set; }

        public Transform Transform { get; private set; }
        public GameObject controllerObject;
        public VoiceClient voiceClient;

        // state
        private Station currentStation;

        public Station CurrentStation
        {
            get => !currentStation ? CurrentWorldManager.GetStationByType(StationType.None) : currentStation;
            set => currentStation = value;
        }

        public StationType CurrentStationType => !currentStation ? StationType.None : currentStation.Type;

        [NonSerialized] public bool IsMoving;
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

        protected virtual void AwakeClient()
        {
        }

        private void Start()
        {
            CurrentWorldManager.RegisterEvent(new GameEvent(photonView.Owner, GameEventType.OnJoinedGame,
                GameEventResult.Success));
            UpdateCustomProperties(photonView.Owner.CustomProperties, true);

            StartClient();
        }

        protected virtual void StartClient()
        {
        }

        private void Update()
        {
            UpdateClient();
        }

        protected virtual void UpdateClient()
        {
        }

        private void OnDestroy()
        {
                if (CurrentWorldManager)
                {
                OnDestroySaveCamera();
                CurrentWorldManager.RegisterEvent(new GameEvent(photonView.Owner, GameEventType.OnLeftGame,
                        GameEventResult.Success));
                }
            OnDestroyClient();
        }
        protected virtual void OnDestroySaveCamera()
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
            if (!IsMine && properties.TryGetValue("stationType", out object stationType))
            {
                OnStationChanged((StationType)stationType);
            }

            if (properties.TryGetValue("daysHospitalized", out object daysHospitalized))
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
            GameEvent gEvent = new GameEvent(info.Sender, data);
            CurrentWorldManager.RegisterEvent(gEvent);
        }
    }
}