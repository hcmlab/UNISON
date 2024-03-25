using System.Collections;
using Backend;
using Photon.Pun;
using PUN;
using Settings;
using TMPro;
using UI.Auxiliary;
using UI.Dialogs;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using AudioSettings = Settings.AudioSettings;

namespace UI.Menu
{
    public class MainMenu : MonoBehaviour
    {
        public PunManager punManager;
        [SerializeField] private GameObject generalBar;
        [SerializeField] private GameObject lobbyBar;
        [SerializeField] private Button[] avatarMenuButtons;
        [SerializeField] private Button[] quitButtons;
        [SerializeField] private Button[] audioMenuButtons;
        [SerializeField] private Button[] controlMenuButtons;
        [SerializeField] private Button[] settingsButtons;
        [SerializeField] private CustomButton hostButton;
        [SerializeField] private CustomButton joinButton;
        [SerializeField] private CustomButton lobbyButton;
        [SerializeField] private GameObject nameInputField;
        [SerializeField] private GameObject instanceText;

        [SerializeField] private GeneralMenu generalMenu;
        [SerializeField] private AdminMenu adminMenu;
        [SerializeField] private LobbyMenu lobbyMenu;
        [SerializeField] private ControlMenu controlMenu;
        [SerializeField] private SettingsMenu settingsMenu;
        [SerializeField] private AudioMenu audioMenu;
        [SerializeField] private AvatarMenu avatarMenu;

        [SerializeField] private GameObject defaultLowerBar;
        [SerializeField] private GameObject rotatedLowerBar;
        [SerializeField] private Sprite defaultBackground;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image background;
        [SerializeField] private CanvasHandler canvasHandler;
        [SerializeField] private TMP_Text gameNameVersionText;

        [SerializeField] private Button englishLanguageButton;
        [SerializeField] private CanvasGroup englishTransparencyImage;
        [SerializeField] private Button germanLanguageButton;
        [SerializeField] private CanvasGroup germanTransparencyImage;

        public const string EnglishLanguage = "english";
        public const string GermanLanguage = "german";
        public static string CurrentLanguage { get; private set; }

        private bool gameStarted;
        private YesNoDialog exitDialog;
        private LoginDialog loginDialog;

        private void Awake()
        {
            AddAllListeners();
        }

        private void Start()
        {
            if (LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.Locales[0])
            {
                UpdateLanguage(EnglishLanguage);
            }
            else if (LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.Locales[1])
            {
                UpdateLanguage(GermanLanguage);
            }
            else
            {
                UpdateLanguage(EnglishLanguage);
            }

            englishLanguageButton.onClick.AddListener(() => { UpdateLanguage(EnglishLanguage); });
            germanLanguageButton.onClick.AddListener(() => { UpdateLanguage(GermanLanguage); });

            gameNameVersionText.text = $"{Application.productName} - Version {Application.version}";
            DisableAll();
            generalBar.SetActive(true);
            lobbyBar.SetActive(false);
        }

        private void UpdateLanguage(string language)
        {
            switch (language)
            {
                case EnglishLanguage:
                    englishTransparencyImage.alpha = 0f;
                    germanTransparencyImage.alpha = 0.5f;
                    LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[0];
                    CurrentLanguage = EnglishLanguage;
                    break;

                case GermanLanguage:
                    englishTransparencyImage.alpha = 0.5f;
                    germanTransparencyImage.alpha = 0f;
                    LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[1];
                    CurrentLanguage = GermanLanguage;
                    break;

                default:
                    throw new UnityException($"Unknown language \"{language}\"!");
            }
        }

        private void OnEnable()
        {
            if (PhotonNetwork.InRoom)
            {
                SwitchToLobbyMode();
            }
            else
            {
                SwitchToNormalMode();
            }
        }

        private void Update()
        {
            // TODO: Optimize with subscriber
            if (punManager.GameStarted && !gameStarted)
            {
                SwitchToLobbyMode();
                gameStarted = true;
                SwitchBackground(true);
                foreach (Button b in avatarMenuButtons)
                    b.gameObject.SetActive(false);
            }
            else if (!punManager.GameStarted && gameStarted)
            {
                SwitchToNormalMode();
                gameStarted = false;
                SwitchBackground(false);
                foreach (Button b in avatarMenuButtons)
                    b.gameObject.SetActive(true);
            }

            CheckAspectRatio();
        }

        private void CheckAspectRatio()
        {
            defaultLowerBar.SetActive(punManager.mainCamera.aspect >= 1.7);
            rotatedLowerBar.SetActive(punManager.mainCamera.aspect < 1.7);
        }

        public void SetLobbyRejoining(bool rejoining)
        {
            lobbyMenu.GetComponent<LobbyMenu>().Rejoining = rejoining;
        }

        public void SwitchToLobbyMode()
        {
            DisableAll();
            lobbyMenu.gameObject.SetActive(true);
            lobbyBar.SetActive(true);
            generalBar.SetActive(false);
            instanceText.SetActive(true);
        }

        public void SwitchToNormalMode()
        {
            if (lobbyMenu.gameObject.activeSelf)
            {
                DisableAll();
            }

            lobbyBar.SetActive(false);
            generalBar.SetActive(true);
        }

        public void ResetPlayerName()
        {
            TMP_InputField playerNameInputField = nameInputField.GetComponent<TMP_InputField>();
            playerNameInputField.text = "";
        }

        private void SwitchBackground(bool inGame)
        {
            backgroundImage.sprite = inGame ? null : defaultBackground;
            backgroundImage.color = inGame ? new Color(1, 1, 1, 0.6f) : new Color(1, 1, 1, 1);
            background.color = inGame ? new Color(1, 1, 1, 0.6f) : new Color(1, 1, 1, 0.82f);
        }

        private void OnGeneral()
        {
            ToggleButtons(false);

            TMP_InputField playerNameInputField = nameInputField.GetComponent<TMP_InputField>();
            playerNameInputField.interactable = false;

            if (BackendConnection.IsLoggedInAsPlayer())
            {
                StartCoroutine(Connect(SwitchToGeneralMenu));
            }
            else
            {
                BackendConnection.Logout();
                ResetPlayerName();

                loginDialog = canvasHandler.InitializeLoginDialog((playerName, playerID, token) =>
                {
                    BackendConnection.LoginAsPlayer(playerID, token);
                    playerNameInputField.text = playerName;

                    StartCoroutine(Connect(SwitchToGeneralMenu));
                    Destroy(loginDialog.gameObject);
                }, () =>
                {
                    playerNameInputField.interactable = true;
                    ToggleButtons(true);
                    Destroy(loginDialog.gameObject);
                });
            }
        }

        private void SwitchToGeneralMenu()
        {
            DisableAll();
            generalMenu.gameObject.SetActive(true);
            nameInputField.SetActive(true);
        }

        private IEnumerator Connect(UnityAction action)
        {
            LoadingDialog loadingDialog = canvasHandler.InitializeLoadingDialog(true);
            yield return BackendConnection.CheckIsInternetAvailable(internetAvailable =>
            {
                if (internetAvailable)
                {
                    StartCoroutine(BackendConnection.CheckIsBackendAvailable(backendAvailable =>
                    {
                        if (backendAvailable)
                        {
                            if (AudioSettings.AudioData == null || AudioSettings.AudioData.InputDevice == null)
                            {
                                canvasHandler.InitializeOkDialog(LocalizationUtility.GetLocalizedString("warningTitle"),
                                    LocalizationUtility.GetLocalizedString("noInputDeviceWarning"),
                                    LocalizationUtility.GetLocalizedString("ok"), FeedbackType.Negative, null);
                            }

                            action.Invoke();
                        }
                        else
                        {
                            canvasHandler.InitializeOkDialog(
                                LocalizationUtility.GetLocalizedString("criticalErrorTitle"),
                                LocalizationUtility.GetLocalizedString("backendConnectionError", "contactAdmin"),
                                LocalizationUtility.GetLocalizedString("ok"), FeedbackType.Negative, null);
                        }
                    }));
                }
                else
                {
                    canvasHandler.InitializeOkDialog(LocalizationUtility.GetLocalizedString("criticalErrorTitle"),
                        LocalizationUtility.GetLocalizedString("internetConnectionError", "contactAdmin"),
                        LocalizationUtility.GetLocalizedString("ok"), FeedbackType.Negative, null);
                }

                Destroy(loadingDialog.gameObject);
                ToggleButtons(true);
            });
        }

        private void OnAdmin()
        {
            ToggleButtons(false);

            StartCoroutine(Connect(SwitchToAdminMenu));
        }

        private void SwitchToAdminMenu()
        {
            DisableAll();
            adminMenu.gameObject.SetActive(true);
            nameInputField.SetActive(true);
        }

        private void OnLobby()
        {
            ToggleButtons(false);
            DisableAll();
            lobbyMenu.gameObject.SetActive(true);
            instanceText.SetActive(true);
            ToggleButtons(true);
        }

        private void OnControl()
        {
            DisableAll();
            controlMenu.gameObject.SetActive(true);
        }

        private void OnAudio()
        {
            DisableAll();
            audioMenu.gameObject.SetActive(true);
        }

        private void OnAvatar()
        {
            DisableAll();
            avatarMenu.gameObject.SetActive(true);
        }

        private void OnSettings()
        {
            DisableAll();
            settingsMenu.gameObject.SetActive(true);
        }

        private void OnQuit()
        {
            if (exitDialog != null) return;
            GetComponent<CanvasGroup>().interactable = false;
            exitDialog = canvasHandler.InitializeMenuYesNoDialog(LocalizationUtility.GetLocalizedString("exitTitle"),
                LocalizationUtility.GetLocalizedString("exitMessage"), LocalizationUtility.GetLocalizedString("yes"),
                LocalizationUtility.GetLocalizedString("no"), Exit, () =>
                {
                    GetComponent<CanvasGroup>().interactable = true;
                    Destroy(exitDialog.gameObject);
                });
        }

        private static void Exit()
        {
            LoadSave.SaveAudioDataToFile(AudioSettings.AudioData);
            LoadSave.SaveControlDataToFile(ControlSettings.ControlData);
            LoadSave.SaveAvatarCustomizationToFile(AvatarSettings.AvatarCustomization);
            Application.Quit();
        }

        private void DisableAll()
        {
            generalMenu.gameObject.SetActive(false);
            adminMenu.gameObject.SetActive(false);
            controlMenu.gameObject.SetActive(false);
            audioMenu.gameObject.SetActive(false);
            lobbyMenu.gameObject.SetActive(false);
            avatarMenu.gameObject.SetActive(false);
            settingsMenu.gameObject.SetActive(false);
            nameInputField.SetActive(false);
            instanceText.SetActive(false);
        }

        private void ToggleButtons(bool value)
        {
            joinButton.UpdateButtonState(value
                ? ButtonState.Active
                : ButtonState.Inactive);

            hostButton.UpdateButtonState(value
                ? ButtonState.Active
                : ButtonState.Inactive);

            lobbyButton.UpdateButtonState(value
                ? ButtonState.Active
                : ButtonState.Inactive);
        }

        private void AddAllListeners()
        {
            joinButton.OnClick.AddListener(OnGeneral);
            hostButton.OnClick.AddListener(OnAdmin);

            lobbyButton.OnClick.AddListener(OnLobby);

            foreach (Button b in quitButtons)
            {
                b.onClick.AddListener(OnQuit);
            }

            foreach (Button b in controlMenuButtons)
            {
                b.onClick.AddListener(OnControl);
            }

            foreach (Button b in audioMenuButtons)
            {
                b.onClick.AddListener(OnAudio);
            }

            foreach (Button b in avatarMenuButtons)
            {
                b.onClick.AddListener(OnAvatar);
            }

            foreach (Button b in settingsButtons)
            {
                b.onClick.AddListener(OnSettings);
            }
        }
    }
}