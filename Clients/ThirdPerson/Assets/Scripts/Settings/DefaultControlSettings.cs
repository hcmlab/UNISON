using UnityEngine;

namespace Settings
{
    [CreateAssetMenu(fileName = "DefaultControlSettings", menuName = "ScriptableObjects/PlayerControlSettings")]
    public class DefaultControlSettings : ScriptableObject
    {
        [SerializeField] private KeyCode primaryForward;
        [SerializeField] private KeyCode secondaryForward;
        [SerializeField] private KeyCode primaryLeft;
        [SerializeField] private KeyCode secondaryLeft;
        [SerializeField] private KeyCode primaryBackward;
        [SerializeField] private KeyCode secondaryBackward;
        [SerializeField] private KeyCode primaryRight;
        [SerializeField] private KeyCode secondaryRight;
        [SerializeField] private KeyCode interact;
        [SerializeField] private KeyCode confirmDialog;
        [SerializeField] private KeyCode cancelDialog;
        [SerializeField] private KeyCode toggleMenu;
        [SerializeField] private KeyCode toggleVoice;
        [SerializeField] private KeyCode openMap;
        [SerializeField] private KeyCode callForHelp;

        [SerializeField] private KeyCode sprint;
        [SerializeField] private KeyCode pushToTalkGlobal;
        [SerializeField] private KeyCode toggleStats;

        public KeyCode PrimaryForward => primaryForward;
        public KeyCode SecondaryForward => secondaryForward;
        public KeyCode PrimaryLeft => primaryLeft;
        public KeyCode SecondaryLeft => secondaryLeft;
        public KeyCode PrimaryBackward => primaryBackward;
        public KeyCode SecondaryBackward => secondaryBackward;
        public KeyCode PrimaryRight => primaryRight;
        public KeyCode SecondaryRight => secondaryRight;
        public KeyCode Interact => interact;
        public KeyCode ConfirmDialog => confirmDialog;
        public KeyCode CancelDialog => cancelDialog;
        public KeyCode ToggleMenu => toggleMenu;
        public KeyCode ToggleVoice => toggleVoice;
        public KeyCode OpenMap => openMap;
        public KeyCode CallForHelp => callForHelp;

        public KeyCode Sprint => sprint;
        public KeyCode PushToTalkGlobal => pushToTalkGlobal;
        public KeyCode ToggleStats => toggleStats;
    }
}