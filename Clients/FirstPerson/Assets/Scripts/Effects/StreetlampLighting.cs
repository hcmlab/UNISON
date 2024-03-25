using UnityEngine;
using World;

namespace Effects
{
    public class StreetlampLighting : MonoBehaviour, IStateSubscriber<DayState>
    {
        private WorldManager worldManager;
        public Light point;
        public Light spot;
        public float pointIntensity = 1;
        public float spotIntensity = .6f;

        private void Awake()
        {
            worldManager = GetComponentInParent<WorldManager>();
        }

        private void Start()
        {
            worldManager.SubscribeToDayState(this, true);
        }

        private void OnDestroy()
        {
            worldManager.UnsubscribeFromDayState(this);
        }

        public void OnUpdated(DayState dayState, DayState oldDayState)
        {
            switch (dayState)
            {
                case DayState.Dusk:
                case DayState.Evening:
                    point.intensity = pointIntensity;
                    spot.intensity = spotIntensity;
                    break;
                default:
                    point.intensity = 0;
                    spot.intensity = 0;
                    break;
            }
        }
    }
}