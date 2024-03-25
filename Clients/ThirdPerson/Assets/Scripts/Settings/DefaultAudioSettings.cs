using UnityEngine;

namespace Settings
{
    /// <summary>
    ///     This class holds default values for audio settings from a scriptable object.
    /// </summary>
    [CreateAssetMenu(fileName = "DefaultAudioSettings", menuName = "ScriptableObjects/PlayerAudioSettings")]
    public class DefaultAudioSettings : ScriptableObject
    {
        [SerializeField] private float volumeEffects;
        [SerializeField] private float volumeAmbience;
        [SerializeField] private float volumeVoice;

        public float VolumeEffects => volumeEffects;
        public float VolumeAmbience => volumeAmbience;
        public float VolumeVoice => volumeVoice;
    }
}