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

        private List<string> labels = new List<string>();
        
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

        public void ToggleToggleVisibility(bool value) => toggleGroup.gameObject.SetActive(value);

        public void UpdateOptions(List<(string, UnityAction<bool>)> labelsAndOnValueChangedListeners)
        {
            labels.Clear();
            
            foreach ((string label, UnityAction<bool> onValueChangedListener) in labelsAndOnValueChangedListeners)
            {
                labels.Add(label);
                
                GameObject customToggle = Instantiate(customTogglePrefab, toggleGroup.transform);

                customToggle.GetComponentInChildren<TMP_Text>().text = label;

                Toggle toggle = customToggle.GetComponent<Toggle>();
                toggle.group = toggleGroup;
                if (onValueChangedListener != null)
                {
                    toggle.onValueChanged.AddListener(onValueChangedListener);
                }
            }
        }

        public void SetSelectionByLabel(string label)
        {
            Toggle[] ts = GetComponentsInChildren<Toggle>();
            foreach (Toggle toggle in ts)
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
            string selectedLabel = GetLabelOfSelection();
            
            for (int i = 0; i < labels.Count; ++i)
            {
                string label = labels[i];
                if (selectedLabel == label)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}