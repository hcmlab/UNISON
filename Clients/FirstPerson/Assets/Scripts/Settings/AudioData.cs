using System;

namespace Settings
{
    /// <summary>
    /// Class for serialization.
    /// </summary>
    [Serializable]
    public class AudioData
    {
        public float VolumeEffects { get; set; }
        public float VolumeAmbience { get; set; }
        public float VolumeVoice { get; set; }
        public string InputDevice { get; set; }

        public AudioData()
        {
        } // Necessary for deserialization

        public AudioData(DefaultAudioSettings defaultAudioSettings)
        {
            VolumeEffects = defaultAudioSettings.VolumeEffects;
            VolumeAmbience = defaultAudioSettings.VolumeAmbience;
            VolumeVoice = defaultAudioSettings.VolumeVoice;
        }
    }
}