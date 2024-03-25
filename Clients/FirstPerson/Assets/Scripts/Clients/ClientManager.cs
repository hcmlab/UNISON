using System;
using System.Collections.Generic;
using Effects;
using Photon.Pun;
using Stations;
using UnityEngine;
using PUN;
using Settings;
using UI;
using World;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Clients
{
    public abstract class ClientManager : MonoBehaviour, IStateSubscriber<DayState>
    {


        // references
        [NonSerialized] public WorldManager WorldManager;
        protected GameObject Client;
        public abstract PunClient PunClient { get; }


        public Offset focusOffset;
        public Offset cameraOffset;

        // parameters & prefabs
        [SerializeField] private GameObject clientPrefab;

        [SerializeField] public GameSettings gameSettings;




        // state
        private List<IStateSubscriber<Station>> stationSubscribers;
        [NonSerialized] public bool IsClientMouseOnScrollingHUD;

        // getters
        protected bool CanInteract => !WorldManager.canvasHandler.GameLocked;

        private void Awake()
        {
            stationSubscribers = new List<IStateSubscriber<Station>>();
        }

        public void Setup()
        {
            Client = InstantiateClient(clientPrefab);
            SetupClient();
            //SubscribeToStation(PunClient.voiceClient);


            //WorldManager.mainCamera.transform.rotation = PunClient.transform.rotation;
            //WorldManager.mainCamera.transform.position = PunClient.transform.GetChild(0).GetChild(0).position;
            //WorldManager.mainCamera.transform.parent = PunClient.transform.GetChild(0).GetChild(0);


            WorldManager.SubscribeToDayState(this);
            WorldManager.SetPlayerProperties(new Hashtable()
            {
                { "customization", AvatarSettings.AvatarCustomization.Serialize() }
            });
        }

        protected virtual void SetupClient()
        {
        }

        /*void FixedUpdate()
        {
            if (CanInteract && !PunClient.IsHospitalized)
            {
                Vector3 movement = CalcMovementVector();
                PunClient.IsMoving = movement.magnitude > 0.01f;
                HandleMovement(movement);
            }
            else
            {
                PunClient.IsMoving = false;
            }
        }*/

        void Update()
        {
            //HandleZoom();
            UpdateClient();
            if (CanInteract && !PunClient.IsHospitalized)
            {
                Vector3 movement = CalcMovementVector();
                PunClient.IsMoving = movement.magnitude > 0.01f;
                HandleMovement(movement);
            }
            else
            {
                PunClient.IsMoving = false;
            }
        }

        protected virtual void UpdateClient()
        {
        }

        void OnDestroy()
        {
            //focusOffset.Transform = null;
            //focusOffset.Transform = null;
            // Vllt auskommentieren
            //UnsubscribeFromStation(PunClient.voiceClient);
            WorldManager.UnsubscribeFromDayState(this);

            OnDestroyClient();
        }

        protected virtual void OnDestroyClient()
        {
        }

        private static GameObject InstantiateClient(GameObject prefab)
        {
            return PhotonNetwork.IsConnected
                ? PhotonNetwork.Instantiate(prefab.name, Vector3.zero, Quaternion.identity)
                : Instantiate(prefab, Vector3.zero, Quaternion.identity);
        }



        public virtual void OnUpdated(DayState dayState, DayState oldDayState)
        {
        }

        public void SubscribeToStation(IStateSubscriber<Station> subscriber, bool updateInitial = false)
        {
            if (stationSubscribers.Contains(subscriber))
            {
                Debug.LogWarning("Tried to register a station subscriber which has already been registered!");
                return;
            }

            stationSubscribers.Add(subscriber);
            if (updateInitial && PunClient != null)
            {
                subscriber.OnUpdated(PunClient.CurrentStation, PunClient.CurrentStation);
            }
        }

        public void UnsubscribeFromStation(IStateSubscriber<Station> subscriber)
        {
            if (stationSubscribers.Contains(subscriber))
            {
                stationSubscribers.Remove(subscriber);
            }
        }

        protected void SetStation(Station station)
        {
            if (PunClient.CurrentStationType == station.Type)
            {
                return;
            }

            SetStation(station, station.GetRandomSpawnPosition());
        }

        protected void SetStation(Station station, Vector3 position)
        {
            if (PunClient.CurrentStationType == station.Type)
            {
                SetPosition(position);
                return;
            }

            SetStationSky(station);
            SetPosition(position);

            Station oldStation = PunClient.CurrentStation;
            PunClient.CurrentStation = station;
            PunClient.RPC("OnStationChanged", RpcTarget.Others, station.Type);
            PunClient.Player.SetCustomProperties(new Hashtable()
            {
                { "stationType", station.Type }
            });

            foreach (IStateSubscriber<Station> subscriber in stationSubscribers)
            {
                subscriber.OnUpdated(station, oldStation);
            }
        }

        public void FadeToStation(StationType stationType)
        {
            if (PunClient.CurrentStationType == stationType)
            {
                return;
            }

            Station station = WorldManager.GetStationByType(stationType);
            FadeToStation(station, station.GetRandomSpawnPosition());
        }

        public void FadeToStation(Station station, Vector3 position)
        {
            if (PunClient.CurrentStationType == station.Type)
            {
                SetPosition(position);
                return;
            }

            WorldManager.canvasHandler.TransitionCanvas.FadeOut(TransitionType.Position, () =>
            {
                SetStation(station, position);
                WorldManager.canvasHandler.TransitionCanvas.FadeIn(TransitionType.Position);
            });
        }

        private void SetStationSky(Station station)
        {
            WorldManager.GetComponent<Sky>().inInterior = station.IsInterior;
            if (station.IsInterior)
            {
                WorldManager.mainCamera.clearFlags = CameraClearFlags.Color;
                WorldManager.mainCamera.backgroundColor = Color.black;
            }
            else
            {
                WorldManager.mainCamera.clearFlags = CameraClearFlags.Skybox;
            }
        }

        protected abstract void HandleMovement(Vector3 movement);


        private Vector3 CalcMovementVector()
        {
            Transform controller = this.PunClient.transform.GetChild(0);
            Vector3 xMove = Vector3.zero;
            Vector3 zMove = Vector3.zero;
            if (Input.GetKey(ControlSettings.ControlData.PrimaryForward) || Input.GetKey(ControlSettings.ControlData.SecondaryForward))
                zMove += controller.forward;
            if (Input.GetKey(ControlSettings.ControlData.PrimaryBackward) || Input.GetKey(ControlSettings.ControlData.SecondaryBackward))
                zMove -= controller.forward;
            if (Input.GetKey(ControlSettings.ControlData.PrimaryLeft) || Input.GetKey(ControlSettings.ControlData.SecondaryLeft))
                xMove -= controller.right;
            if (Input.GetKey(ControlSettings.ControlData.PrimaryRight) || Input.GetKey(ControlSettings.ControlData.SecondaryRight))
                xMove += controller.right;

            //Vector3 movement = PunClient.transform.GetChild(0).right * xMove + PunClient.transform.GetChild(0).forward * zMove;
            //return movement;
            return xMove + zMove;
        }

        protected virtual void SetPosition(Vector3 position)
        {
            PunClient.transform.position = position;
            if (PunClient.transform.TryGetComponent<CharacterController>(out CharacterController con))
            {
                con.enabled = true;
            }

        }

    }
}