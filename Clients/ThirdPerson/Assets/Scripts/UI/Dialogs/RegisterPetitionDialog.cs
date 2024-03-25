using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.Dialogs
{
    public class RegisterPetitionDialog : MonoBehaviour
    {
        private const KeyCode RegisterKeyCode = KeyCode.Alpha1;
        private const KeyCode DoNotRegisterKeyCode = KeyCode.Alpha2;
        [SerializeField] private Button registerButton;
        [SerializeField] private TMP_Text registerText;
        [SerializeField] private Image registerImage;

        [SerializeField] private Button doNotRegisterButton;
        [SerializeField] private TMP_Text doNotRegisterText;
        [SerializeField] private Image doNotRegisterImage;

        [SerializeField] private TMP_Text petitionText;

        [SerializeField] private TMP_Text timer;
        [SerializeField] private Sprite defaultSprite;
        [SerializeField] private Sprite selectedSprite;
        private UnityAction<bool> countdownOverAction;
        private float endingTime;

        private bool registerPetition;

        private void Update()
        {
            if (endingTime < Time.time)
            {
                countdownOverAction(registerPetition);
                Destroy(gameObject);
            }
            else
            {
                if (Input.GetKeyDown(RegisterKeyCode) && registerButton && registerButton.isActiveAndEnabled &&
                    registerButton.IsInteractable())
                {
                    registerButton.onClick.Invoke();
                }
                else if (Input.GetKeyDown(DoNotRegisterKeyCode) && doNotRegisterButton &&
                         doNotRegisterButton.isActiveAndEnabled && doNotRegisterButton.IsInteractable())
                {
                    doNotRegisterButton.onClick.Invoke();
                }
            }
        }

        public void Setup(string registerPetitionText, UnityAction<bool> newCountdownOverAction, float newEndingTime)
        {
            registerPetition = false;
            countdownOverAction = newCountdownOverAction;
            endingTime = newEndingTime;

            petitionText.text = "\"" + registerPetitionText + "\"";

            registerText.text = LocalizationUtility.AddLocalizedKeyCode(RegisterKeyCode,
                LocalizationUtility.GetLocalizedString("register"));

            doNotRegisterText.text = LocalizationUtility.AddLocalizedKeyCode(DoNotRegisterKeyCode,
                LocalizationUtility.GetLocalizedString("doNotRegister"));

            registerButton.onClick.AddListener(() =>
            {
                registerPetition = true;
                UpdateButtons();
            });
            doNotRegisterButton.onClick.AddListener(() =>
            {
                registerPetition = false;
                UpdateButtons();
            });

            UpdateButtons();

            StartCoroutine(Countdown());
        }

        private IEnumerator Countdown()
        {
            while (isActiveAndEnabled)
            {
                timer.text =
                    LocalizationUtility.GetLocalizedString("cooldownText",
                        new[] { Mathf.Ceil(endingTime - Time.time) });

                yield return new WaitForSeconds(1f);
            }
        }

        private void UpdateButtons()
        {
            registerImage.sprite = registerPetition ? selectedSprite : defaultSprite;
            doNotRegisterImage.sprite = registerPetition ? defaultSprite : selectedSprite;
        }
    }
}