using System.Collections.Generic;
using Stations;
using UI.Dialogs;
using UI.HUD;
using UnityEngine;
using UnityEngine.Events;

namespace UI
{
    public class CanvasHandler : MonoBehaviour
    {
        [SerializeField] private GameObject dialogCanvas;
        [SerializeField] public GameObject importantDialogCanvas;
        [SerializeField] private TransitionCanvas transitionCanvas;
        [SerializeField] private HUDManager hudManager;
        [SerializeField] private GameObject menu;

        [SerializeField] private RegisterPetitionDialog registerPetitionDialog;
        [SerializeField] private EveningActivityDialog eveningActivityDialog;
        [SerializeField] private SimpleDialog simpleDialog;
        [SerializeField] private OkDialog bigOkDialog;
        [SerializeField] private OkDialog okDialog;
        [SerializeField] private LoginDialog loginDialog;
        [SerializeField] private YesNoDialog yesNoDialog;
        [SerializeField] private HelpDeskDialog helpDeskDialog;
        [SerializeField] private GraphsDialog graphsDialog;
        [SerializeField] private SelectDialog selectDialog;
        [SerializeField] private SelectAndSpendDialog selectAndSpendDialog;
        [SerializeField] private VoteDialog voteDialog;
        [SerializeField] private PetitionDialog petitionDialog;
        [SerializeField] private LoadingDialog loadingDialog;
        [SerializeField] private ToggleDialog toggleDialog;
        [SerializeField] private ToggleDialog bigToggleDialog;
        [SerializeField] private ToggleAndSpendDialog toggleAndSpendDialog;

        public TransitionCanvas TransitionCanvas => transitionCanvas;
        public HUDManager HUDManager => hudManager;
        private bool hudToggled = true;
        public bool MenuOpened => menu.gameObject.activeSelf;
        private bool InGameDialog => dialogCanvas.transform.childCount > 0;
        private bool InImportantDialog => importantDialogCanvas.transform.childCount > 0;
        private bool InDialog => InGameDialog || InImportantDialog;
        public bool GameLocked => MenuOpened || InDialog || transitionCanvas.InTransition;

        public void Update()
        {
            UpdateDialogCanvas();

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            // TODO: To control settings?
            if (Input.GetKeyDown(KeyCode.F1) && !MenuOpened)
            {
                hudToggled = !hudToggled;
                ToggleHUDManager(hudToggled);
            }
#endif
        }

        // TODO: Call on changes in importantDialogCanvas
        private void UpdateDialogCanvas()
        {
            ToggleDialogCanvas(!InImportantDialog);
        }

        public LoadingDialog InitializeLoadingDialog(bool isInMenu = false)
        {
            return InitializeDialog(loadingDialog, isInMenu ? importantDialogCanvas : dialogCanvas);
        }

        public RegisterPetitionDialog InitializeRegisterPetitionDialog(string petitionText,
            UnityAction<bool> newCountdownOverAction, float newEndingTime)
        {
            RegisterPetitionDialog newDialog = InitializeDialog(registerPetitionDialog, importantDialogCanvas);
            newDialog.Setup(petitionText, newCountdownOverAction, newEndingTime);
            return newDialog;
        }

        public EveningActivityDialog InitializeEveningActivityDialog(UnityAction homeButtonListener,
            UnityAction townHallButtonListener, UnityAction loungeButtonListener, StationType selectedStation,
            float endingTime, bool isInLockdown)
        {
            ClearDialogCanvas();

            EveningActivityDialog newDialog = InitializeDialog(eveningActivityDialog);
            newDialog.Setup(homeButtonListener, townHallButtonListener, loungeButtonListener, selectedStation,
                endingTime, isInLockdown);
            return newDialog;
        }

        public SimpleDialog InitializeSimpleDialog(string newTitle, string newMessage)
        {
            ClearDialogCanvas();

            SimpleDialog newDialog = InitializeDialog(simpleDialog);
            newDialog.Setup(newTitle, newMessage);
            return newDialog;
        }

        public OkDialog InitializeBigOkDialog(string newTitle, string newMessage, string yesButtonText,
            UnityAction confirmAction)
        {
            OkDialog newDialog = InitializeDialog(bigOkDialog);
            newDialog.Setup(newTitle, newMessage, yesButtonText, confirmAction);
            return newDialog;
        }

        public OkDialog InitializeOkDialog(string newTitle, string newMessage, string yesButtonText,
            UnityAction confirmAction)
        {
            OkDialog newDialog = InitializeDialog(okDialog, importantDialogCanvas);
            newDialog.Setup(newTitle, newMessage, yesButtonText, confirmAction);
            return newDialog;
        }

        public OkDialog InitializeOkDialog(string newTitle, string newMessage, string yesButtonText,
            FeedbackType feedback, UnityAction confirmAction)
        {
            OkDialog newDialog = InitializeDialog(okDialog, importantDialogCanvas);
            newDialog.Setup(newTitle, newMessage, yesButtonText, feedback, confirmAction);
            return newDialog;
        }

        public LoginDialog InitializeLoginDialog(UnityAction<string, int, string> successAction,
            UnityAction cancelAction)
        {
            LoginDialog newDialog = InitializeDialog(loginDialog, importantDialogCanvas);
            newDialog.Setup(successAction, cancelAction);
            return newDialog;
        }

        public YesNoDialog InitializeYesNoDialog(string newTitle, string newMessage, string yesButtonText,
            string noButtonText, UnityAction confirmAction, UnityAction cancelAction)
        {
            YesNoDialog newDialog = InitializeDialog(yesNoDialog);
            newDialog.Setup(newTitle, newMessage, yesButtonText, noButtonText, confirmAction, cancelAction);
            return newDialog;
        }

        public YesNoDialog InitializeMenuYesNoDialog(string newTitle, string newMessage, string yesButtonText,
            string noButtonText, UnityAction confirmAction, UnityAction cancelAction)
        {
            YesNoDialog newDialog = InitializeDialog(yesNoDialog, importantDialogCanvas);
            newDialog.Setup(newTitle, newMessage, yesButtonText, noButtonText, confirmAction, cancelAction);
            return newDialog;
        }

        public HelpDeskDialog InitializeHelpDeskDialog(UnityAction confirmAction = null)
        {
            HelpDeskDialog newDialog = InitializeDialog(helpDeskDialog);
            newDialog.Setup(confirmAction);
            return newDialog;
        }

        public GraphsDialog InitializeGraphsDialog(string newTitle, string newMessage, string yesButtonText,
            UnityAction confirmAction, Dictionary<string, string> options)
        {
            GraphsDialog newDialog = InitializeDialog(graphsDialog);
            newDialog.Setup(newTitle, newMessage, yesButtonText, confirmAction, options);
            return newDialog;
        }

        public SelectDialog InitializeSelectDialog(string newTitle, string newMessage, string yesButtonText,
            string noButtonText, UnityAction confirmAction, UnityAction cancelAction, List<string> options,
            UnityAction<int> onValueChangedListener = null)
        {
            SelectDialog newDialog = InitializeDialog(selectDialog);
            newDialog.Setup(newTitle, newMessage, yesButtonText, noButtonText, confirmAction, cancelAction, options,
                onValueChangedListener);
            return newDialog;
        }

        public SelectAndSpendDialog InitializeSelectAndSpendDialog(string newTitle, string newMessage,
            string yesButtonText, string noButtonText, UnityAction confirmAction, UnityAction cancelAction,
            List<string> options, UnityAction<int> onValueChangedListener = null)
        {
            SelectAndSpendDialog newDialog = InitializeDialog(selectAndSpendDialog);
            newDialog.Setup(newTitle, newMessage, yesButtonText, noButtonText, confirmAction, cancelAction, options,
                onValueChangedListener);
            return newDialog;
        }

        public ToggleDialog InitializeToggleDialog(string newTitle, string newMessage, string yesButtonText,
            string noButtonText, UnityAction confirmAction, UnityAction cancelAction,
            List<(string, UnityAction<bool>)> labelsAndOnValueChangedListeners, string activeSelection = null)
        {
            ToggleDialog newDialog = InitializeDialog(toggleDialog);
            newDialog.Setup(newTitle, newMessage, yesButtonText, noButtonText, confirmAction, cancelAction,
                labelsAndOnValueChangedListeners, activeSelection);
            return newDialog;
        }

        public ToggleDialog InitializeBigToggleDialog(string newTitle, string newMessage, string yesButtonText,
            string noButtonText, UnityAction confirmAction, UnityAction cancelAction,
            List<(string, UnityAction<bool>)> labelsAndOnValueChangedListeners, string activeSelection = null)
        {
            ToggleDialog newDialog = InitializeDialog(bigToggleDialog);
            newDialog.Setup(newTitle, newMessage, yesButtonText, noButtonText, confirmAction, cancelAction,
                labelsAndOnValueChangedListeners, activeSelection);
            return newDialog;
        }

        public ToggleAndSpendDialog InitializeToggleAndSpendDialog(string newTitle, string newMessage,
            string yesButtonText, string noButtonText, UnityAction confirmAction, UnityAction cancelAction,
            List<(string, UnityAction<bool>)> labelsAndOnValueChangedListeners, string activeSelection = null)
        {
            ToggleAndSpendDialog newDialog = InitializeDialog(toggleAndSpendDialog);
            newDialog.Setup(newTitle, newMessage, yesButtonText, noButtonText, confirmAction, cancelAction,
                labelsAndOnValueChangedListeners, activeSelection);
            return newDialog;
        }

        public VoteDialog InitializeVoteDialog(string newTitle, string newMessage, string yesButtonText,
            string noButtonText, UnityAction confirmAction, UnityAction cancelAction)
        {
            VoteDialog newDialog = InitializeDialog(voteDialog);
            newDialog.Setup(newTitle, newMessage, yesButtonText, noButtonText, confirmAction, cancelAction);
            return newDialog;
        }

        public PetitionDialog InitializePetitionDialog(string newTitle, string newMessage, string yesButtonText,
            string noButtonText, UnityAction confirmAction, UnityAction cancelAction)
        {
            PetitionDialog newDialog = InitializeDialog(petitionDialog);
            newDialog.Setup(newTitle, newMessage, yesButtonText, noButtonText, confirmAction, cancelAction);
            return newDialog;
        }

        private T InitializeDialog<T>(T dialog, GameObject canvas = null) where T : MonoBehaviour
        {
            canvas ??= dialogCanvas;
            return Instantiate(dialog, canvas.transform);
        }

        public void ClearDialogCanvas()
        {
            foreach (Transform child in dialogCanvas.transform)
            {
                Destroy(child.gameObject);
            }
        }

        public void ToggleMenu(bool toggle)
        {
            menu.SetActive(toggle);
            ToggleDialogCanvas(!toggle);
            ToggleHUDManager(!toggle);
        }

        private void ToggleDialogCanvas(bool toggle)
        {
            CanvasGroup group = dialogCanvas.GetComponent<CanvasGroup>();
            group.alpha = toggle ? 1 : 0;
            group.interactable = toggle;
        }

        private void ToggleHUDManager(bool toggle)
        {
            CanvasGroup group = hudManager.GetComponent<CanvasGroup>();
            group.alpha = toggle ? 1 : 0;
            group.interactable = toggle;
            hudToggled = toggle;
        }
    }
}