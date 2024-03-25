using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Voice.Unity;
using Settings;
using UI.Auxiliary;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using AudioSettings = Settings.AudioSettings;

namespace UI.Menu
{
    public class AudioMenu : MonoBehaviour
    {
        [SerializeField] private SliderInputField volumeEffectsSliderInputField;
        [SerializeField] private SliderInputField volumeAmbienceSliderInputField;
        [SerializeField] private SliderInputField volumeVoiceSliderInputField;
        [SerializeField] private SliderInputField voiceDetectionThresholdSliderInputField;
        [SerializeField] private StringSwitch inputDeviceSwitch;
        [SerializeField] private Button resetToDefaultButton;
        [SerializeField] private Recorder recorder;
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private DefaultAudioSettings defaultAudioSettings;
        [SerializeField] private GameSettings gameSettings;

        private const int VolumeSliderStepSize = 2;

        private List<string> deviceList;

        private void Awake()
        {
            deviceList = new List<string>();
            resetToDefaultButton.onClick.AddListener(OnResetToDefault);
        }

        private void Start()
        {
            //inputDeviceSwitch = GameObject.FindGameObjectWithTag("InputDeviceSwitch").GetComponent<StringSwitch>();
            InitMenuItems();
        }

        private void OnDisable()
        {
            LoadSave.SaveAudioDataToFile(AudioSettings.AudioData);
        }

        private void Update()
        {
            if (Microphone.devices.ToList() != deviceList)
            {
                InitMicrophoneItemSwitch();
            }
        }

        private void UpdateEffectsVolume(float value)
        {
            AudioSettings.AudioData.VolumeEffects = value;
            float scaledVolume = AudioSettings.ScaleVolume(value / gameSettings.MaxVolume);
            audioMixer.SetFloat("SFXVolume", scaledVolume);
            audioMixer.SetFloat("ClientVolume", scaledVolume);
        }

        private void UpdateAmbienceVolume(float value)
        {
            AudioSettings.AudioData.VolumeAmbience = value;
            audioMixer.SetFloat("AmbienceVolume", AudioSettings.ScaleVolume(value / gameSettings.MaxVolume));
        }

        private void UpdateVoiceVolume(float value)
        {
            AudioSettings.AudioData.VolumeVoice = value;
            audioMixer.SetFloat("VoiceVolume", AudioSettings.ScaleVolume(value / gameSettings.MaxVolume));
        }

        private void UpdateVoiceDetectionThreshold(float value)
        {
            recorder.VoiceDetectionThreshold = value / 100f;
        }

        private static void UpdateMicrophoneDevice(string device)
        {
            if (device != null)
            {
                AudioSettings.AudioData.InputDevice = device;
            }
        }

        private void InitMenuItems()
        {
            volumeEffectsSliderInputField.SetupSlider(gameSettings.MinVolume, gameSettings.MaxVolume,
                AudioSettings.AudioData.VolumeEffects, UpdateEffectsVolume, VolumeSliderStepSize);

            volumeAmbienceSliderInputField.SetupSlider(gameSettings.MinVolume, gameSettings.MaxVolume,
                AudioSettings.AudioData.VolumeAmbience, UpdateAmbienceVolume, VolumeSliderStepSize);

            volumeVoiceSliderInputField.SetupSlider(gameSettings.MinVolume, gameSettings.MaxVolume,
                AudioSettings.AudioData.VolumeVoice, UpdateVoiceVolume, VolumeSliderStepSize);

            float currentThreshold = recorder.VoiceDetectionThreshold * 100f;
            int adjustedCurrentThreshold =
                Math.Max(Math.Min((int)currentThreshold, gameSettings.MaxVoiceDetectionThreshold),
                    gameSettings.MinVoiceDetectionThreshold);
            adjustedCurrentThreshold = 1;
            UpdateVoiceDetectionThreshold(adjustedCurrentThreshold);

            voiceDetectionThresholdSliderInputField.SetupSlider(gameSettings.MinVoiceDetectionThreshold,
                gameSettings.MaxVoiceDetectionThreshold, adjustedCurrentThreshold, UpdateVoiceDetectionThreshold);

            InitMicrophoneItemSwitch();
        }

        private void InitMicrophoneItemSwitch()
        {
            deviceList = Microphone.devices.ToList();
            if (Microphone.devices.Any())
            {
                Dictionary<string, string> itemByNames = new Dictionary<string, string>();
                foreach (string device in Microphone.devices)
                {
                    itemByNames[device] = device;
                }

                inputDeviceSwitch.SetupItemSwitch(itemByNames, AudioSettings.AudioData.InputDevice,
                    UpdateMicrophoneDevice);
            }
            else
            {
                inputDeviceSwitch.GetComponent<ItemSwitch<string>>().SetupItemSwitch(null, null, UpdateMicrophoneDevice,
                    LocalizationUtility.GetLocalizedString("noInputDeviceAvailable"));
            }
        }

        private void OnResetToDefault()
        {
            AudioSettings.AudioData = new AudioData(defaultAudioSettings);
            InitMenuItems();
        }
    }
}