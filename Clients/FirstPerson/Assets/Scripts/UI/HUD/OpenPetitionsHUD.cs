using System.Collections;
using System.Globalization;
using Backend;
using TMPro;
using UnityEngine;
using World;

namespace UI.HUD
{
    public class OpenPetitionsHUD : MonoBehaviourHUD, IStateSubscriber<DayState>
    {
        [SerializeField] private CanvasGroup canvasGroup;

        [SerializeField] private TMP_Text textStart;
        [SerializeField] private TMP_Text number;
        [SerializeField] private TMP_Text textEnd;
        
        private DayState? currentDayState;

        public override void OnBeingEnabled(WorldManager worldManager)
        {
            base.OnBeingEnabled(worldManager);

            CurrentWorldManager.SubscribeToDayState(this, true);
            StartCoroutine(UpdateInfo());
        }

        public override void OnBeingDisabled()
        {
            CurrentWorldManager.UnsubscribeFromDayState(this);
        }

        private IEnumerator UpdateInfo()
        {
            while (isActiveAndEnabled)
            {
                if (currentDayState is DayState.Daylight)
                {
                    yield return BackendConnection.GetOpenPetitions(response =>
                    {
                        if (response.Data.Count == 0)
                        {
                            textStart.text = "";
                            number.text = "";
                            textEnd.text = "";

                            canvasGroup.alpha = 0;
                            canvasGroup.interactable = false;
                        }
                        else
                        {
                            textStart.text = LocalizationUtility.GetLocalizedString("openPetitionsTextStart");
                            number.text = response.Data.Count.ToString(CultureInfo.InvariantCulture);
                            textEnd.text = LocalizationUtility.GetLocalizedString("openPetitionsTextEnd");

                            canvasGroup.alpha = 1;
                            canvasGroup.interactable = true;
                        }
                    });
                }
                else
                {
                    canvasGroup.alpha = 0;
                    canvasGroup.interactable = false;
                }

                yield return new WaitForSeconds(1f);
            }
        }

        public void OnUpdated(DayState newDayState, DayState _)
        {
            currentDayState = newDayState;
        }
    }
}