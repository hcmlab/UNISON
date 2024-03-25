namespace Stations
{
    public class AmbienceSoundManager : SoundManager, IStateSubscriber<Station>
    {
        private Station currentStation;
        private bool isAmbienceEnabled;

        private void Update()
        {
            // TODO: Optimize with subscriber
            if (currentStation && isAmbienceEnabled != currentStation.IsAmbienceEnabled)
            {
                UpdateAmbience();
            }
        }

        public void OnUpdated(Station station, Station oldStation)
        {
            currentStation = station;
            UpdateAmbience();
        }

        private void UpdateAmbience()
        {
            isAmbienceEnabled = currentStation.IsAmbienceEnabled;
            if (currentStation.AmbienceClip == null || !isAmbienceEnabled)
            {
                Stop();
            }
            else
            {
                if (currentStation.Type == StationType.Hospital)
                {
                    PlayOneFadingOut(currentStation.AmbienceClip);
                }
                else
                {
                    PlayLooped(currentStation.AmbienceClip);
                }
            }
        }

        protected override void ConfigAudioSource()
        {
            audioSource.playOnAwake = true;

            audioSource.priority = 192;
            audioSource.reverbZoneMix = 0f;
            audioSource.spatialBlend = 0f;

            audioSource.minDistance = 0f;
        }
    }
}