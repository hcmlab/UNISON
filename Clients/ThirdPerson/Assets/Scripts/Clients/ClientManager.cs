using System;
using System.Collections.Generic;
using Effects;
using ExitGames.Client.Photon;
using Photon.Pun;
using PUN;
using Settings;
using Stations;
using UI;
using UnityEngine;
using World;

namespace Clients
{
    public abstract class ClientManager : MonoBehaviour, IStateSubscriber<DayState>
    {
        // parameters & prefabs
        [SerializeField] private GameObject clientPrefab;

        [SerializeField] public GameSettings gameSettings;

        [SerializeField] private float cameraDistance = 20f;
        [SerializeField] private float minCameraDistance = 10f;
        [SerializeField] private float maxCameraDistance = 30f;
        private Offset cameraOffset;
        protected GameObject Client;

        private Offset focusOffset;
        [NonSerialized] public bool IsClientMouseOnScrollingHUD;

        // state
        private List<IStateSubscriber<Station>> stationSubscribers;

        // references
        [NonSerialized] public WorldManager WorldManager;
        public abstract PunClient PunClient { get; }

        // getters
        protected bool CanInteract => !WorldManager.canvasHandler.GameLocked;

        private void Awake()
        {
            stationSubscribers = new List<IStateSubscriber<Station>>();
        }

        private void Update()
        {
            HandleZoom();

            UpdateClient();
        }

        private void FixedUpdate()
        {
            if (CanInteract && !PunClient.IsHospitalized)
            {
                var movement = CalcMovementVector();
                PunClient.IsMoving = movement.magnitude > 0.01f;
                HandleMovement(movement);
            }
            else
            {
                PunClient.IsMoving = false;
            }
        }

        private void OnDestroy()
        {
            focusOffset.Transform = null;

            WorldManager.UnsubscribeFromDayState(this);

            OnDestroyClient();
        }

        public virtual void OnUpdated(DayState dayState, DayState oldDayState)
        {
        }

        public void Setup()
        {
            Client = InstantiateClient(clientPrefab);
            SetupClient();

            focusOffset = WorldManager.focusObject.GetComponent<Offset>();
            focusOffset.Transform = PunClient.Transform;

            cameraOffset = WorldManager.mainCamera.GetComponent<Offset>();
            cameraOffset.offset.z = -cameraDistance;

            WorldManager.SubscribeToDayState(this);
            WorldManager.SetPlayerProperties(new Hashtable
            {
                { "customization", AvatarSettings.AvatarCustomization.Serialize() }
            });
        }

        protected virtual void SetupClient()
        {
        }

        protected virtual void UpdateClient()
        {
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

        private void HandleZoom()
        {
            if (!CanInteract || IsClientMouseOnScrollingHUD)
            {
                return;
            }

            var scroll = Input.mouseScrollDelta.y;
            if (scroll == 0)
            {
                return;
            }

            cameraDistance = Mathf.Clamp(cameraDistance - scroll, minCameraDistance, maxCameraDistance);
            cameraOffset.offset.z = -cameraDistance;
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

            var oldStation = PunClient.CurrentStation;
            PunClient.CurrentStation = station;
            PunClient.RPC("OnStationChanged", RpcTarget.Others, station.Type);
            PunClient.Player.SetCustomProperties(new Hashtable
            {
                { "stationType", station.Type }
            });

            foreach (var subscriber in stationSubscribers)
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

            var station = WorldManager.GetStationByType(stationType);
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

        private static Vector3 CalcMovementVector()
        {
            var vertical = 0.0f;
            if (Input.GetKey(ControlSettings.ControlData.PrimaryForward)
                || Input.GetKey(ControlSettings.ControlData.SecondaryForward))
            {
                vertical += 1.0f;
            }

            if (Input.GetKey(ControlSettings.ControlData.PrimaryBackward)
                || Input.GetKey(ControlSettings.ControlData.SecondaryBackward))
            {
                vertical -= 1.0f;
            }

            var horizontal = 0.0f;
            if (Input.GetKey(ControlSettings.ControlData.PrimaryLeft)
                || Input.GetKey(ControlSettings.ControlData.SecondaryLeft))
            {
                horizontal -= 1.0f;
            }

            if (Input.GetKey(ControlSettings.ControlData.PrimaryRight)
                || Input.GetKey(ControlSettings.ControlData.SecondaryRight))
            {
                horizontal += 1.0f;
            }

            /*if (Input.GetKey(ControlSettings.controlData.up))
                move += 1;
            if (Input.GetKey(ControlSettings.controlData.down))
                move -= 1;*/

            return new Vector3(horizontal, 0, vertical).normalized;
        }

        protected virtual void SetPosition(Vector3 position)
        {
            PunClient.transform.position = position;
        }
    }
}