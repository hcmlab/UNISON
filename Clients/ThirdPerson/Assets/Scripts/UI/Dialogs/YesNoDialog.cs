using System.Linq;
using Settings;
using UI.Auxiliary;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UI.Dialogs
{
    public class YesNoDialog : OkDialog
    {
        public CustomButton noButton;

        protected override void Start()
        {
            if (noButton && !HasBeenSetup)
            {
                Debug.LogError($"{GetType()} dialog has not been setup!");
                noButton.OnClick.AddListener(() => Destroy(gameObject));
            }
        }

        protected override void Update()
        {
            base.Update();

            if (noButton && noButton.isActiveAndEnabled && Input.GetKeyDown(ControlSettings.ControlData.CancelDialog) &&
                !EventSystem.current.currentSelectedGameObject)
            {
                noButton.OnClick.Invoke();
            }
        }

        public void Setup(string newTitle, string newMessage, string yesButtonText, string noButtonText,
            UnityAction confirmAction, UnityAction cancelAction)
        {
            base.Setup(newTitle, newMessage, yesButtonText, confirmAction);

            if (noButton)
            {
                if (noButtonText != null)
                {
                    noButton.Text =
                        LocalizationUtility.AddLocalizedKeyCode(ControlSettings.ControlData.CancelDialog, noButtonText);
                }

                noButton.OnClick.RemoveAllListeners();
                if (cancelAction != null)
                {
                    noButton.OnClick.AddListener(cancelAction);
                }
            }
        }

        public void ResetNoButtonOnClickListener()
        {
            noButton.OnClick.RemoveAllListeners();
        }

        public void AddNoButtonOnClickListener(UnityAction cancelAction)
        {
            noButton.OnClick.AddListener(cancelAction);
        }

        public void UpdateNoButtonText(string noButtonText)
        {
            noButton.Text =
                LocalizationUtility.AddLocalizedKeyCode(ControlSettings.ControlData.CancelDialog, noButtonText);
        }

        public void ToggleNoButtonVisibility(bool value)
        {
            noButton.gameObject.SetActive(value);
        }

        public override void ToggleButtons(bool value, params CustomButton[] loadingButtons)
        {
            base.ToggleButtons(value, loadingButtons);
            if (noButton)
            {
                noButton.UpdateButtonState(value
                    ? ButtonState.Active
                    : loadingButtons.Contains(noButton)
                        ? ButtonState.Loading
                        : ButtonState.Inactive);
            }
        }
    }
}