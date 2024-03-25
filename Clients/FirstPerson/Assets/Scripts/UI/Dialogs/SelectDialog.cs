using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace UI.Dialogs
{
    public class SelectDialog : YesNoDialog, IToggleButtonsDialog, ISelectDialog
    {
        [SerializeField] private TMP_Dropdown selectionDropdown;

        public void Setup(string newTitle, string newMessage, string yesButtonText, string noButtonText,
            UnityAction confirmAction, UnityAction cancelAction, List<string> options,
            UnityAction<int> onValueChangedListener = null)
        {
            base.Setup(newTitle, newMessage, yesButtonText, noButtonText, confirmAction, cancelAction);

            if (options != null)
            {
                UpdateOptions(options);
            }

            if (onValueChangedListener != null)
            {
                AddOnValueChangedListener(onValueChangedListener);
            }
        }

        public void UpdateOptions(List<string> options)
        {
            selectionDropdown.ClearOptions();
            selectionDropdown.AddOptions(options);
        }

        public void AddOnValueChangedListener(UnityAction<int> call) =>
            selectionDropdown.onValueChanged.AddListener(call);

        public int GetIndexOfSelection() => selectionDropdown.value;

        public void SetIndexOfSelection(int index) => selectionDropdown.value = index;

        public void ToggleSelect(bool value)
        {
            selectionDropdown.interactable = value;
        }
    }
}