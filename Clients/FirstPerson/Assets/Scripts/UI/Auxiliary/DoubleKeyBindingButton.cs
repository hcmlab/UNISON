using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Auxiliary
{
    public class DoubleKeyBindingButton : KeyBindingButton
    {
        [SerializeField] private Button secondaryButton;
        [SerializeField] private Button secondaryLargeButton;

        private bool checkForNewSecondaryKey;
        private Action<KeyCode> secondaryOnChangeListener;

        protected override void Start()
        {
            base.Start();

            secondaryButton.onClick.AddListener(() => { checkForNewSecondaryKey = true; });
            secondaryLargeButton.onClick.AddListener(() => { checkForNewSecondaryKey = true; });
        }

        protected override void OnGUI()
        {
            base.OnGUI();

            if (checkForNewSecondaryKey && Event.current.isKey)
            {
                secondaryOnChangeListener.Invoke(Event.current.keyCode);
                checkForNewSecondaryKey = false;
            }
        }

        public void SetupKeyBindingButton(string newKeyCodeText, KeyCode initialPrimaryKeyCode,
            Action<KeyCode> newPrimaryOnChangeListener, KeyCode initialSecondaryKeyCode,
            Action<KeyCode> newSecondaryOnChangeListener)
        {
            base.SetupKeyBindingButton(newKeyCodeText, initialPrimaryKeyCode, newPrimaryOnChangeListener);
            UpdateSecondaryKeyCode(initialSecondaryKeyCode);
            secondaryOnChangeListener = newSecondaryOnChangeListener;
        }

        public void UpdateSecondaryKeyCode(KeyCode newKeyCode)
        {
            UpdateKeyCode(newKeyCode, gameSettings, secondaryButton, secondaryLargeButton);
        }
    }
}