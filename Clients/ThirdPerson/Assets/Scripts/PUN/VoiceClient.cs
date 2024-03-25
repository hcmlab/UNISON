using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using JetBrains.Annotations;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using Settings;
using UnityEngine;
using UnityEngine.Audio;
using AudioSettings = Settings.AudioSettings;

namespace PUN
{
    public enum VoiceState
    {
        Idle,
        Speaking,
        Mute,
        Unavailable
    }

    public enum VoiceRegion
    {
        Global,
        Local
    }

    [Serializable]
    public struct VoiceClientOptions
    {
        public AudioMixerGroup mixerGroup;
        public float minDistance;
        public float maxDistance;
    }

    public class VoiceClient : MonoBehaviourPun
    {
        [SerializeField] private VoiceClientOptions masterOptions;
        [SerializeField] private VoiceClientOptions playerOptions;

        private readonly List<byte> groupsToAdd = new List<byte>();
        private readonly List<byte> groupsToRemove = new List<byte>();
        private AudioSource audioSource;
        private PhotonVoiceView photonVoiceView;
        private PunManager punManager;
        private Speaker speaker;
        private List<IStateSubscriber<VoiceRegion>> speakingRegionSubscribers;
        private byte[] subscribers;

        private List<IStateSubscriber<VoiceState>> voiceStateSubscribers;
        public VoiceState VoiceState { get; private set; }

        private Recorder Recorder => photonVoiceView.RecorderInUse;
        private bool IsMine => photonView.IsMine;
        private bool IsMaster => photonView.Owner.IsMasterClient;
        private byte TargetInterestGroup => (byte)photonView.Owner.ActorNumber;

        public VoiceRegion SpeakingRegion { get; private set; }
        public bool MasterInRange { get; private set; }

        private void Awake()
        {
            punManager = GameObject.FindWithTag("PunManager").GetComponent<PunManager>();
            photonVoiceView = GetComponent<PhotonVoiceView>();
            speaker = GetComponent<Speaker>();
            audioSource = GetComponent<AudioSource>();
            voiceStateSubscribers = new List<IStateSubscriber<VoiceState>>();
            speakingRegionSubscribers = new List<IStateSubscriber<VoiceRegion>>();
        }

        private void Start()
        {
            punManager.RegisterVoiceClient(this);
            if (IsMine)
            {
                gameObject.layer = LayerMask.NameToLayer("VoiceSender");

                speaker.enabled = false;
                audioSource.enabled = false;
            }

            var options = IsMaster ? masterOptions : playerOptions;
            audioSource.outputAudioMixerGroup = options.mixerGroup;

            audioSource.minDistance = options.minDistance;
            audioSource.maxDistance = options.maxDistance;

            var c = GetComponent<SphereCollider>();
            if (c)
            {
                c.radius = options.maxDistance;
            }

            UpdateVoiceState(VoiceState.Idle);
            if (photonView.Owner.CustomProperties.TryGetValue("mute", out var mute))
            {
                OnMuted((bool)mute);
            }
        }

        private void Update()
        {
            if (IsMine)
            {
                UpdateMine();
            }
            else
            {
                UpdateOthers();
            }
        }

        private void OnDestroy()
        {
            punManager.UnregisterVoiceClient(this);
        }

        private void OnTriggerEnter(Collider other)
        {
            var client = other.GetComponent<VoiceClient>();
            if (client == null)
            {
                return;
            }

            var group = client.TargetInterestGroup;
            if (group == TargetInterestGroup || group == 0)
            {
                return;
            }

            if (client.IsMaster)
            {
                MasterInRange = true;
            }

            if (groupsToRemove.Contains(group))
            {
                groupsToRemove.Remove(group);
            }

            if (!groupsToAdd.Contains(group))
            {
                groupsToAdd.Add(group);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var client = other.GetComponent<VoiceClient>();
            if (client == null)
            {
                return;
            }

            var group = client.TargetInterestGroup;
            if (group == TargetInterestGroup || group == 0)
            {
                return;
            }

            if (client.IsMaster)
            {
                MasterInRange = false;
            }

            if (groupsToAdd.Contains(group))
            {
                groupsToAdd.Remove(group);
            }

            if (!groupsToRemove.Contains(group))
            {
                groupsToRemove.Add(group);
            }
        }

        private void UpdateMine()
        {
            if (PhotonVoiceNetwork.Instance.ClientState != ClientState.Joined)
            {
                return;
            }

            UpdateSubscribers();
            if (Recorder == null || AudioSettings.AudioData.InputDevice == null)
            {
                if (VoiceState != VoiceState.Unavailable)
                {
                    UpdateVoiceState(VoiceState.Unavailable);
                    photonView.RPC("OnUnavailable", RpcTarget.Others, true);
                }

                return;
            }

            if (VoiceState == VoiceState.Unavailable)
            {
                UpdateVoiceState(VoiceState.Idle);
                photonView.RPC("OnUnavailable", RpcTarget.Others, false);
            }

            if (Recorder.UnityMicrophoneDevice != AudioSettings.AudioData.InputDevice)
            {
                Recorder.UnityMicrophoneDevice = AudioSettings.AudioData.InputDevice;
            }

            if (Recorder.RequiresRestart)
            {
                Recorder.RestartRecording();
            }

            if (Input.GetKeyDown(ControlSettings.ControlData.ToggleVoice))
            {
                UpdateVoiceState(VoiceState switch
                {
                    VoiceState.Mute => VoiceState.Idle,
                    _ => VoiceState.Mute
                });

                photonView.RPC("OnMuted", RpcTarget.Others, VoiceState == VoiceState.Mute);
                photonView.Owner.SetCustomProperties(new Hashtable
                {
                    { "mute", VoiceState == VoiceState.Mute }
                });
            }

            UpdateSpeakingRegion(IsMaster && Input.GetKey(ControlSettings.ControlData.PushToTalkGlobal)
                ? VoiceRegion.Global
                : punManager.GameVoiceRegion);

            if (VoiceState == VoiceState.Mute)
            {
                Recorder.TransmitEnabled = false;
                Recorder.IsRecording = false;
                return;
            }

            var isRecording =
                SpeakingRegion == VoiceRegion.Global || (subscribers != null && subscribers.Length > 0);
            if (Recorder.IsRecording != isRecording)
            {
                Recorder.TransmitEnabled = isRecording;
                Recorder.IsRecording = isRecording;
            }

            UpdateVoiceState(Recorder.VoiceDetector.Detected ? VoiceState.Speaking : VoiceState.Idle);
        }

        private void UpdateOthers()
        {
            if (VoiceState == VoiceState.Mute || VoiceState == VoiceState.Unavailable)
            {
                return;
            }

            UpdateVoiceState(speaker.IsPlaying ? VoiceState.Speaking : VoiceState.Idle);
            audioSource.spatialBlend = punManager.GameVoiceRegion == VoiceRegion.Global || IsMaster ? 0 : 1;
        }

        private void UpdateVoiceState(VoiceState voiceState)
        {
            if (VoiceState == voiceState)
            {
                return;
            }

            var oldVoiceState = VoiceState;
            VoiceState = voiceState;
            foreach (var subscriber in voiceStateSubscribers)
            {
                subscriber.OnUpdated(VoiceState, oldVoiceState);
            }
        }

        private void UpdateSpeakingRegion(VoiceRegion speakingRegion)
        {
            if (SpeakingRegion == speakingRegion)
            {
                return;
            }

            var oldSpeakingRegion = SpeakingRegion;
            SpeakingRegion = speakingRegion;

            Recorder.InterestGroup = SpeakingRegion switch
            {
                VoiceRegion.Local => TargetInterestGroup,
                _ => 0
            };

            foreach (var subscriber in speakingRegionSubscribers)
            {
                subscriber.OnUpdated(SpeakingRegion, oldSpeakingRegion);
            }
        }

        public void SubscribeToVoiceState(IStateSubscriber<VoiceState> subscriber, bool updateInitial = false)
        {
            if (voiceStateSubscribers.Contains(subscriber))
            {
                Debug.LogWarning("Tried to register a voice state subscriber which has already been registered!");
                return;
            }

            voiceStateSubscribers.Add(subscriber);
            if (updateInitial)
            {
                subscriber.OnUpdated(VoiceState, VoiceState);
            }
        }

        public void UnsubscribeFromVoiceState(IStateSubscriber<VoiceState> subscriber)
        {
            if (voiceStateSubscribers.Contains(subscriber))
            {
                voiceStateSubscribers.Remove(subscriber);
            }
        }

        public void SubscribeToSpeakingRegion(IStateSubscriber<VoiceRegion> subscriber, bool updateInitial = false)
        {
            if (speakingRegionSubscribers.Contains(subscriber))
            {
                Debug.LogWarning("Tried to register a voice region subscriber which has already been registered!");
                return;
            }

            speakingRegionSubscribers.Add(subscriber);
            if (updateInitial)
            {
                subscriber.OnUpdated(SpeakingRegion, SpeakingRegion);
            }
        }

        public void UnsubscribeFromSpeakingRegion(IStateSubscriber<VoiceRegion> subscriber)
        {
            if (speakingRegionSubscribers.Contains(subscriber))
            {
                speakingRegionSubscribers.Remove(subscriber);
            }
        }

        [PunRPC]
        [UsedImplicitly]
        public void OnMuted(bool isMute)
        {
            UpdateVoiceState(isMute ? VoiceState.Mute : VoiceState.Idle);
        }

        [PunRPC]
        [UsedImplicitly]
        public void OnUnavailable(bool isUnavailable)
        {
            UpdateVoiceState(isUnavailable ? VoiceState.Unavailable : VoiceState.Idle);
        }

        private void UpdateSubscribers()
        {
            if (punManager.GameVoiceRegion == VoiceRegion.Global)
            {
                return;
            }

            if (groupsToAdd.Count == 0 && groupsToRemove.Count == 0)
            {
                return;
            }

            byte[] toAdd = null;
            byte[] toRemove = null;
            if (groupsToAdd.Count > 0)
            {
                toAdd = groupsToAdd.ToArray();
            }

            if (groupsToRemove.Count > 0)
            {
                toRemove = groupsToRemove.ToArray();
            }

            if (!PhotonVoiceNetwork.Instance.Client.OpChangeGroups(toRemove, toAdd))
            {
                return;
            }

            if (subscribers != null)
            {
                var list = new List<byte>();
                foreach (var t in subscribers)
                {
                    list.Add(t);
                }

                foreach (var t in groupsToRemove)
                {
                    if (list.Contains(t))
                    {
                        list.Remove(t);
                    }
                }

                foreach (var t in groupsToAdd)
                {
                    if (!list.Contains(t))
                    {
                        list.Add(t);
                    }
                }

                subscribers = list.ToArray();
            }
            else
            {
                subscribers = toAdd;
            }

            groupsToAdd.Clear();
            groupsToRemove.Clear();
        }
    }
}