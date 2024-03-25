using PUN;
using UnityEngine;

namespace Clients
{
    public class PlayerSoundManager : SoundManager
    {
        public AudioClip[] steps;

        protected override void ConfigAudioSource()
        {
            if (GetComponentInParent<PunClient>().IsMine)
            {
                audioSource.spatialBlend = 0;
            }
        }

        public void Play(string clip)
        {
            switch (clip)
            {
                case "Step":
                    PlayRandom(steps);
                    break;
            }
        }
    }
}