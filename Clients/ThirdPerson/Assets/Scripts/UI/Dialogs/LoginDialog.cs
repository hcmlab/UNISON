using Backend;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UI.Dialogs
{
    public class LoginDialog : YesNoDialog
    {
        [SerializeField] private TMP_InputField playerName;
        [SerializeField] private TMP_InputField playerPassword;
        [SerializeField] private TMP_Text errorMessage;

        private UnityAction<string, int, string> successAction;

        protected override void Update()
        {
            base.Update();

            if (Input.GetKeyDown(KeyCode.Tab) && EventSystem.current.currentSelectedGameObject == playerName.gameObject)
            {
                playerPassword.ActivateInputField();
                playerPassword.Select();
                playerPassword.text = "";
            }
        }

        public void Setup(UnityAction<string, int, string> newSuccessAction, UnityAction cancelAction)
        {
            base.Setup(LocalizationUtility.GetLocalizedString("loginTitle"),
                LocalizationUtility.GetLocalizedString("loginMessage"), LocalizationUtility.GetLocalizedString("login"),
                LocalizationUtility.GetLocalizedString("cancel"), TryToLogin, cancelAction);

            errorMessage.text = "";
            successAction = newSuccessAction;
        }

        private void TryToLogin()
        {
            ToggleButtons(false, yesButton);
            errorMessage.text = "";

            var possiblePlayerName = playerName.text;
            if (possiblePlayerName.Trim() == "")
            {
                errorMessage.text = LocalizationUtility.GetLocalizedString("missingNameError");
                ToggleButtons(true);
                return;
            }

            if (possiblePlayerName == BackendConnection.MasterClientName)
            {
                errorMessage.text = LocalizationUtility.GetLocalizedString("masterClientNameAsPlayerError");
                ToggleButtons(true);
                return;
            }

            var possiblePlayerPassword = playerPassword.text;
            if (possiblePlayerPassword.Trim() == "")
            {
                errorMessage.text = LocalizationUtility.GetLocalizedString("missingPasswordError");
                ToggleButtons(true);
                return;
            }

            StartCoroutine(BackendConnection.Login(response =>
            {
                if (response.IsSuccess())
                {
                    if (response.Data != null)
                    {
                        successAction(possiblePlayerName, response.Data.ID, response.Data.Token);
                    }
                    else
                    {
                        errorMessage.text = LocalizationUtility.GetLocalizedString("wrongNamePasswordError");
                        ToggleButtons(true);
                    }
                }
                else
                {
                    errorMessage.text = LocalizationUtility.GetLocalizedString("backendError", "contactAdmin");
                    ToggleButtons(true);
                }
            }, possiblePlayerName, possiblePlayerPassword, true));
        }
    }
}