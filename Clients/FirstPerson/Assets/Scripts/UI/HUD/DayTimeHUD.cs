using System.Globalization;
using TMPro;
using UI.Auxiliary;
using UnityEngine;
using World;

namespace UI.HUD
{
    /// <summary>
    ///     Shows the progress of the current [DayState] and the day count
    /// </summary>
    public class DayTimeHUD : MonoBehaviourHUD, IStateSubscriber<DayState>
    {
        [SerializeField] private TMP_Text dayTimeText;
        public CircularProgressObject circularProgressObject;
        private Sprite dusk;
        private Sprite moon;

        private Sprite sun;

        private void Awake()
        {
            sun = Resources.Load<Sprite>("Sprites/sun");
            dusk = Resources.Load<Sprite>("Sprites/daynight");
            moon = Resources.Load<Sprite>("Sprites/moon");
        }

        private void Update()
        {
            circularProgressObject.SetPercentage(CurrentWorldManager.DaytimeProgress);
        }

        public void OnUpdated(DayState newDayState, DayState _)
        {
            var s = newDayState switch
            {
                DayState.Dusk => dusk,
                DayState.Evening => moon,
                DayState.NextDay => moon,
                _ => sun
            };

            circularProgressObject.SetImageSource(s, Color.yellow);
            dayTimeText.text = CurrentWorldManager.GameRound.ToString(CultureInfo.InvariantCulture);
        }

        public override void OnBeingEnabled(WorldManager worldManager)
        {
            base.OnBeingEnabled(worldManager);
            CurrentWorldManager.SubscribeToDayState(this, true);
        }

        public override void OnBeingDisabled()
        {
            base.OnBeingDisabled();
            CurrentWorldManager.UnsubscribeFromDayState(this);
        }
    }
}
