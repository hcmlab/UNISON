using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Auxiliary
{
    public class ItemSwitch<T> : MonoBehaviour
    {
        [SerializeField] private TMP_Text content;
        [SerializeField] private Button leftButton;
        [SerializeField] private Button rightButton;

        private List<string> items;
        private string selectedItem;

        public void Awake()
        {
            leftButton.onClick.AddListener(SwitchToPreviousItem);
            rightButton.onClick.AddListener(SwitchToNextItem);
        }

        public void SetupItemSwitch(Dictionary<string, T> itemByNames, T selected, Action<T> action,
            string notAvailableText = null)
        {
            notAvailableText ??= LocalizationUtility.GetLocalizedString("noItemAvailable");

            items = itemByNames?.Keys.ToList();

            selectedItem = itemByNames?.FirstOrDefault(x => x.Value.Equals(selected)).Key;

            if (items == null || items.Count == 0)
            {
                content.text = notAvailableText;
            }
            else
            {
                content.text = selectedItem;
            }

            leftButton.onClick.AddListener(() =>
                action.Invoke(itemByNames != null ? itemByNames[selectedItem] : default));
            rightButton.onClick.AddListener(() =>
                action.Invoke(itemByNames != null ? itemByNames[selectedItem] : default));
        }

        private void SwitchToNextItem()
        {
            if (items == null)
            {
                return;
            }

            if (items.Count <= 1)
            {
                return;
            }

            int index;

            if (items.Contains(selectedItem))
            {
                index = items.IndexOf(selectedItem);
            }
            else
            {
                index = 0;
                selectedItem = items[0];
                content.text = items[0];
            }

            if (index >= items.Count - 1)
            {
                index = 0;
            }
            else
            {
                ++index;
            }

            selectedItem = items[index];
            content.text = selectedItem;
        }

        private void SwitchToPreviousItem()
        {
            if (items == null)
            {
                return;
            }

            if (items.Count <= 1)
            {
                return;
            }

            int index;

            if (items.Contains(selectedItem))
            {
                index = items.IndexOf(selectedItem);
            }
            else
            {
                index = 0;
                selectedItem = items[0];
                content.text = items[0];
            }

            if (index - 1 < 0)
            {
                index = items.Count - 1;
            }
            else
            {
                --index;
            }

            selectedItem = items[index];
            content.text = selectedItem;
        }
    }
}