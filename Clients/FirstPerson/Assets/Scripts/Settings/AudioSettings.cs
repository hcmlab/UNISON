using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

namespace Settings
{
    /// <summary>
    /// This class holds the audio settings of the game.
    /// </summary>
    public class AudioSettings : MonoBehaviour
    {
        [SerializeField] private DefaultAudioSettings defaultAudioSettings;
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private GameSettings gameSettings;

        public static AudioData AudioData;

        private void Awake()
        {
            AudioData = LoadAudioData();
        }

        private void Start()
        {
            audioMixer.SetFloat("SFXVolume", ScaleVolume(AudioData.VolumeEffects / gameSettings.MaxVolume));
            audioMixer.SetFloat("ClientVolume", ScaleVolume(AudioData.VolumeEffects / gameSettings.MaxVolume));
            audioMixer.SetFloat("AmbienceVolume", ScaleVolume(AudioData.VolumeAmbience / gameSettings.MaxVolume));
            audioMixer.SetFloat("VoiceVolume", ScaleVolume(AudioData.VolumeVoice / gameSettings.MaxVolume));
        }

        private void FixedUpdate()
        {
            if (Microphone.devices.Any())
            {
                // Check if selected device is still connected
                if (!Microphone.devices.ToList().Contains(AudioData.InputDevice))
                {
                    AudioData.InputDevice = Microphone.devices[0];
                }
            }
            else
            {
                AudioData.InputDevice = null;
            }
        }

        private AudioData LoadAudioData()
        {
            AudioData data = LoadSave.LoadAudioDataFromFile() ?? new AudioData(defaultAudioSettings);
            if (data.InputDevice != null && Microphone.devices.Contains(data.InputDevice))
            {
                // Selected device from last session is still available
                return data;
            }

            if (data.InputDevice != null && !Microphone.devices.Contains(data.InputDevice))
            {
                // Selected device from last session is no longer available
                data.InputDevice = Microphone.devices.Length >= 1 ? Microphone.devices[0] : null;
                return data;
            }

            if (data.InputDevice == null && Microphone.devices.Length >= 1)
            {
                // No device selected last time, selecting default device
                data.InputDevice = Microphone.devices[0];
            }

            return data;
        }

        public static float ScaleVolume(float value)
        {
            return Mathf.Max(-80f, -8f * Mathf.Log(value, 0.5f) + 10f);
        }
    }
}