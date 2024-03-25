using System.Linq;
using Settings;
using UI.Auxiliary;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.Dialogs
{
    public class OkDialog : SimpleDialog
    {
        public CustomButton yesButton;

        [SerializeField] private RawImage feedbackTypeImage;

        [SerializeField] private Texture positiveTexture;
        [SerializeField] private Texture negativeTexture;
        [SerializeField] private Texture neutralTexture;

        [SerializeField] private AudioClip successSound;
        [SerializeField] private AudioClip errorSound;
        [SerializeField] private AudioClip infoSound;

        private AudioSource audioSource;

        protected virtual void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        protected virtual void Start()
        {
            if (yesButton && !HasBeenSetup)
            {
                Debug.LogError($"{GetType()} dialog has not been setup!");
                yesButton.OnClick.AddListener(() => Destroy(gameObject));
            }
        }

        protected virtual void Update()
        {
            if (yesButton && yesButton.isActiveAndEnabled &&
                Input.GetKeyDown(ControlSettings.ControlData.ConfirmDialog))
            {
                yesButton.OnClick.Invoke();
            }
        }

        public void Setup(string newTitle, string newMessage, string yesButtonText, FeedbackType feedback,
            UnityAction confirmAction)
        {
            switch (feedback)
            {
                case FeedbackType.Positive:
                    feedbackTypeImage.texture = positiveTexture;
                    audioSource.PlayOneShot(successSound);
                    break;

                case FeedbackType.Negative:
                    feedbackTypeImage.texture = negativeTexture;
                    audioSource.PlayOneShot(errorSound);
                    break;

                default:
                    feedbackTypeImage.texture = neutralTexture;
                    audioSource.PlayOneShot(infoSound);
                    break;
            }

            base.Setup(newTitle, newMessage);

            if (yesButton)
            {
                if (yesButtonText != null)
                {
                    yesButton.Text =
                        LocalizationUtility.AddLocalizedKeyCode(ControlSettings.ControlData.ConfirmDialog,
                            yesButtonText);
                }

                yesButton.OnClick.RemoveAllListeners();
                if (confirmAction != null)
                {
                    yesButton.OnClick.AddListener(confirmAction);
                }

                yesButton.OnClick.AddListener(() => Destroy(gameObject));
            }
        }

        public void Setup(string newTitle, string newMessage, string yesButtonText, UnityAction confirmAction)
        {
            base.Setup(newTitle, newMessage);

            if (feedbackTypeImage)
            {
                Color color = feedbackTypeImage.color;
                color.a = 0f;
                feedbackTypeImage.color = color;
            }

            audioSource.PlayOneShot(infoSound);

            if (yesButton)
            {
                if (yesButtonText != null)
                {
                    yesButton.Text =
                        LocalizationUtility.AddLocalizedKeyCode(ControlSettings.ControlData.ConfirmDialog,
                            yesButtonText);
                }

                yesButton.OnClick.RemoveAllListeners();
                if (confirmAction != null)
                {
                    yesButton.OnClick.AddListener(confirmAction);
                }
            }
        }

        public void ResetYesButtonOnClickListener() => yesButton.OnClick.RemoveAllListeners();

        public void AddYesButtonOnClickListener(UnityAction confirmAction) =>
            yesButton.OnClick.AddListener(confirmAction);

        public void UpdateYesButtonText(string yesButtonText) => yesButton.Text =
            LocalizationUtility.AddLocalizedKeyCode(ControlSettings.ControlData.ConfirmDialog, yesButtonText);

        public void ToggleYesButtonVisibility(bool value) => yesButton.gameObject.SetActive(value);

        public virtual void ToggleButtons(bool value, params CustomButton[] loadingButtons)
        {
            if (yesButton)
            {
                yesButton.UpdateButtonState(value
                    ? ButtonState.Active
                    : loadingButtons.Contains(yesButton)
                        ? ButtonState.Loading
                        : ButtonState.Inactive);
            }
        }
    }

    public enum FeedbackType
    {
        Positive,
        Negative,
        Neutral
    }
}