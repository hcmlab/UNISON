using UnityEngine;

namespace World
{
    public class WorldSoundManager : SoundManager
    {
        public AudioClip daylight;
        public AudioClip evening;
	
        protected override void ConfigAudioSource()
        {
            audioSource.playOnAwake = true;
		
            audioSource.priority = 192;
            audioSource.reverbZoneMix = 0f;
            audioSource.spatialBlend = 0f;
		
            audioSource.minDistance = 0f;
        }

        public void Play(string clip)
        {
            switch(clip) {
                case "Daylight":
                    PlayOne(daylight);
                    break;
                case "Evening":
                    PlayOne(evening);
                    break;
            }
        }
    }
}