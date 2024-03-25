using System;
using Settings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Auxiliary
{
    public class KeyBindingButton : MonoBehaviour
    {
        [SerializeField] protected GameSettings gameSettings;
        [SerializeField] private TMP_Text keyCodeText;
        [SerializeField] private Button primaryButton;
        [SerializeField] private Button primaryLargeButton;

        private bool checkForNewPrimaryKey;
        private Action<KeyCode> primaryOnChangeListener;

        protected virtual void Start()
        {
            primaryButton.onClick.AddListener(() => { checkForNewPrimaryKey = true; });
            primaryLargeButton.onClick.AddListener(() => { checkForNewPrimaryKey = true; });
        }

        protected virtual void OnGUI()
        {
            if (checkForNewPrimaryKey && Event.current.isKey)
            {
                primaryOnChangeListener.Invoke(Event.current.keyCode);
                checkForNewPrimaryKey = false;
            }
        }

        public void SetupKeyBindingButton(string newKeyCodeText, KeyCode initialPrimaryKeyCode,
            Action<KeyCode> newPrimaryOnChangeListener)
        {
            keyCodeText.text = newKeyCodeText;
            UpdatePrimaryKeyCode(initialPrimaryKeyCode);
            primaryOnChangeListener = newPrimaryOnChangeListener;
        }

        public void UpdatePrimaryKeyCode(KeyCode newKeyCode)
        {
            UpdateKeyCode(newKeyCode, gameSettings, primaryButton, primaryLargeButton);
        }

        protected static void UpdateKeyCode(KeyCode newKeyCode, GameSettings gameSettings, Button button,
            Button largeButton)
        {
            string newKeyText = LocalizationUtility.GetTextForKeyCode(newKeyCode);

            button.GetComponentInChildren<TMP_Text>().text = newKeyText;
            largeButton.GetComponentInChildren<TMP_Text>().text = newKeyText;

            if (newKeyText.Length > gameSettings.SmallKeyCodeMaxLength)
            {
                button.gameObject.SetActive(false);
                largeButton.gameObject.SetActive(true);
            }
            else
            {
                button.gameObject.SetActive(true);
                largeButton.gameObject.SetActive(false);
            }
        }

    }
}