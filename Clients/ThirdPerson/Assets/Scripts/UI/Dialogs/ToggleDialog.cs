using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.Dialogs
{
    public class ToggleDialog : YesNoDialog
    {
        [SerializeField] private GameObject customTogglePrefab;
        [SerializeField] private ToggleGroup toggleGroup;

        private readonly List<string> labels = new List<string>();

        public void Setup(string newTitle, string newMessage, string yesButtonText, string noButtonText,
            UnityAction confirmAction, UnityAction cancelAction,
            List<(string, UnityAction<bool>)> labelsAndOnValueChangedListeners, string activeSelection = null)
        {
            base.Setup(newTitle, newMessage, yesButtonText, noButtonText, confirmAction, cancelAction);

            if (labelsAndOnValueChangedListeners != null)
            {
                UpdateOptions(labelsAndOnValueChangedListeners);
            }

            if (activeSelection != null)
            {
                SetSelectionByLabel(activeSelection);
            }
        }

        public void ToggleToggleVisibility(bool value)
        {
            toggleGroup.gameObject.SetActive(value);
        }

        public void UpdateOptions(List<(string, UnityAction<bool>)> labelsAndOnValueChangedListeners)
        {
            labels.Clear();

            foreach (var (label, onValueChangedListener) in labelsAndOnValueChangedListeners)
            {
                labels.Add(label);

                var customToggle = Instantiate(customTogglePrefab, toggleGroup.transform);

                customToggle.GetComponentInChildren<TMP_Text>().text = label;

                var toggle = customToggle.GetComponent<Toggle>();
                toggle.group = toggleGroup;
                if (onValueChangedListener != null)
                {
                    toggle.onValueChanged.AddListener(onValueChangedListener);
                }
            }
        }

        public void SetSelectionByLabel(string label)
        {
            var ts = GetComponentsInChildren<Toggle>();
            foreach (var toggle in ts)
            {
                if (toggle.GetComponentInChildren<TMP_Text>().text == label)
                {
                    toggle.isOn = true;
                    break;
                }
            }
        }

        public string GetLabelOfSelection()
        {
            return toggleGroup.ActiveToggles().First().GetComponentInChildren<TMP_Text>().text;
        }

        public int GetIndexOfSelection()
        {
            var selectedLabel = GetLabelOfSelection();

            for (var i = 0; i < labels.Count; ++i)
            {
                var label = labels[i];
                if (selectedLabel == label)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}