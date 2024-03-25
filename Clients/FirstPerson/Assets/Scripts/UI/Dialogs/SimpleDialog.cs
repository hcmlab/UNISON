using TMPro;
using UnityEngine;

namespace UI.Dialogs
{
    public class SimpleDialog : MonoBehaviour
    {
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text message;

        protected bool HasBeenSetup { get; private set; }

        public void Setup(string newTitle, string newMessage)
        {
            HasBeenSetup = true;

            title.text = newTitle ?? "";
            message.text = newMessage ?? "";
        }

        public void UpdateDialogTitle(string newTitle) => title.text = newTitle;

        public void UpdateDialogMessage(string newMessage) => message.text = newMessage;
    }
}