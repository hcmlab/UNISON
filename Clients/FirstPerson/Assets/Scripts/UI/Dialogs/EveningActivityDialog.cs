using System.Collections;
using Stations;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.Dialogs
{
    public class EveningActivityDialog : MonoBehaviour
    {
        [SerializeField] private Button loungeButton;
        [SerializeField] private TMP_Text loungeText;
        [SerializeField] private Image loungeImage;

        [SerializeField] private Button homeButton;
        [SerializeField] private TMP_Text homeText;
        [SerializeField] private Image homeImage;

        [SerializeField] private Button townHallButton;
        [SerializeField] private TMP_Text townHallText;
        [SerializeField] private Image townHallImage;

        [SerializeField] private TMP_Text timer;
        [SerializeField] private Sprite defaultSprite;
        [SerializeField] private Sprite selectedSprite;

        private const KeyCode LoungeKeyCode = KeyCode.Alpha1;
        private const KeyCode HomeKeyCode = KeyCode.Alpha2;
        private const KeyCode TownHallKeyCode = KeyCode.Alpha3;

        private float endingTime;

        private void Update()
        {
            if (endingTime < Time.time)
            {
                Destroy(gameObject);
            }
            else
            {
                if (Input.GetKeyDown(LoungeKeyCode) && loungeButton && loungeButton.isActiveAndEnabled &&
                    loungeButton.IsInteractable())
                {
                    loungeButton.onClick.Invoke();
                }
                else if (Input.GetKeyDown(HomeKeyCode) && homeButton && homeButton.isActiveAndEnabled &&
                         homeButton.IsInteractable())
                {
                    homeButton.onClick.Invoke();
                }
                else if (Input.GetKeyDown(TownHallKeyCode) && townHallButton && townHallButton.isActiveAndEnabled &&
                         townHallButton.IsInteractable())
                {
                    townHallButton.onClick.Invoke();
                }
            }
        }

        public void Setup(UnityAction homeButtonListener, UnityAction townHallButtonListener,
            UnityAction loungeButtonListener, StationType selectedStation, float newEndingTime, bool isInLockdown)
        {
            endingTime = newEndingTime;

            loungeText.text =
                LocalizationUtility.AddLocalizedKeyCode(LoungeKeyCode, StationType.Lounge.GetDisplayName());

            homeText.text =
                LocalizationUtility.AddLocalizedKeyCode(HomeKeyCode, StationType.Home.GetDisplayName());

            townHallText.text =
                LocalizationUtility.AddLocalizedKeyCode(TownHallKeyCode, StationType.TownHall.GetDisplayName());

            if (isInLockdown)
            {
                loungeButton.interactable = false;
            }
            else
            {
                loungeButton.onClick.AddListener(loungeButtonListener);
                loungeButton.onClick.AddListener(() => HighlightSelectedButton(StationType.Lounge));
            }

            homeButton.onClick.AddListener(homeButtonListener);
            homeButton.onClick.AddListener(() => HighlightSelectedButton(StationType.Home));

            townHallButton.onClick.AddListener(townHallButtonListener);
            townHallButton.onClick.AddListener(() => HighlightSelectedButton(StationType.TownHall));

            HighlightSelectedButton(selectedStation);

            StartCoroutine(Countdown());
        }

        private IEnumerator Countdown()
        {
            while (isActiveAndEnabled)
            {
                /* timer.text =
                     LocalizationUtility.GetLocalizedString("cooldownText",
                         new[] { Mathf.Ceil(endingTime - Time.time) }); */
                timer.text = Mathf.Ceil(endingTime - Time.time) + "s";
                 yield return new WaitForSeconds(1f);
            }
        }

        private void HighlightSelectedButton(StationType stationType)
        {
            loungeImage.sprite = stationType == StationType.Lounge ? selectedSprite : defaultSprite;
            homeImage.sprite = stationType == StationType.Home ? selectedSprite : defaultSprite;
            townHallImage.sprite = stationType == StationType.TownHall ? selectedSprite : defaultSprite;
        }
    }
}