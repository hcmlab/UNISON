using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Backend;
using Photon.Realtime;
using PUN;
using Sentry;
using Settings;
using TMPro;
using UI.Auxiliary;
using UI.Dialogs;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace UI.Menu
{
    public class AdminMenu : MonoBehaviour
    {
        private const int DurationSliderStepSize = 10;
        private const int LobbySliderStepSize = 1;
        private const int CooldownStepSize = 1;
        private const string Password = "CoronaGame2022";
        [SerializeField] private CanvasHandler canvasHandler;
        [SerializeField] private MainMenu mainMenu;
        [SerializeField] private TMP_InputField nameInputField;
        [SerializeField] private TMP_InputField instanceNameInputField;
        [SerializeField] private SliderInputField lobbySizeSliderInputField;
        [SerializeField] private SliderInputField durationDaySliderInputField;
        [SerializeField] private SliderInputField relaxCooldownSliderInputField;
        [SerializeField] private TMP_InputField passwordInputField;
        [SerializeField] private GameObject loginView;
        [SerializeField] private GameObject createInstanceView;
        [SerializeField] private GameObject loadInstanceView;
        [SerializeField] private Transform instanceScrollView;
        [SerializeField] private InstanceButton instanceButtonPrefab;
        [SerializeField] private CustomButton loginButton;
        [SerializeField] private CustomButton createInstanceButton;
        [SerializeField] private CustomButton clearSaveGamesButton;
        [SerializeField] private GameSettings gameSettings;

        private string adminNameWhenStarted;
        private int daylightDurationWhenStarted;
        private string instanceNameWhenStarted;

        private int lobbySizeWhenStarted;
        private float relaxCooldownInSecondsWhenStarted;

        private void Awake()
        {
            loginButton.OnClick.AddListener(OnLogin);
            createInstanceButton.OnClick.AddListener(OnCreateSession);
            clearSaveGamesButton.OnClick.AddListener(OnClearSaveGames);

            durationDaySliderInputField.SetupSlider(gameSettings.MinDurationDay, gameSettings.MaxDurationDay,
                gameSettings.DefaultDurationDay, sliderStepSize: DurationSliderStepSize, flexibleSliderMax: true);

            lobbySizeSliderInputField.SetupSlider(gameSettings.MinLobbySize, gameSettings.MaxLobbySize,
                gameSettings.DefaultLobbySize, sliderStepSize: LobbySliderStepSize, flexibleSliderMax: true);

            relaxCooldownSliderInputField.SetupSlider(gameSettings.MinCooldown, gameSettings.DefaultDurationEvening,
                gameSettings.DefaultCooldown, sliderStepSize: CooldownStepSize, flexibleSliderMax: true);

            instanceNameInputField.onValueChanged.AddListener(ValidateInstanceName);
            nameInputField.onValueChanged.AddListener(ValidateMasterName);

            ToggleButtons(true);
        }

        private void OnEnable()
        {
            RefreshSaveGameData();
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            passwordInputField.text = Password;
            instanceNameInputField.text = Guid.NewGuid().ToString().Substring(0, gameSettings.MaxInstanceNameLength)
                .Replace('-', '_');
            nameInputField.text = Guid.NewGuid().ToString().Substring(0, gameSettings.MaxPlayerNameLength)
                .Replace('-', ' ');
#endif

            instanceNameInputField.interactable = true;
            nameInputField.interactable = true;

            if (BackendConnection.IsLoggedInAsMasterClient())
            {
                loginView.SetActive(false);
                createInstanceView.SetActive(true);
                loadInstanceView.SetActive(true);
            }
            else
            {
                BackendConnection.Logout();
                mainMenu.ResetPlayerName();

                loginView.SetActive(true);
                createInstanceView.SetActive(false);
                loadInstanceView.SetActive(false);
            }
        }

        private void ValidateMasterName(string masterName)
        {
            var validMasterName = new StringBuilder(masterName.Length);
            var isMasterNameValid = masterName.Length <= gameSettings.MaxPlayerNameLength;
            foreach (var character in masterName)
            {
                if (char.IsLetterOrDigit(character) || character == ' ' || character == '.' || character == ',' ||
                    character == '-' || character == '_' || character == '!' || character == '?' || character == '(' ||
                    character == ')')
                {
                    validMasterName.Append(character);
                }
                else
                {
                    isMasterNameValid = false;
                }
            }

            // Avoid expensive UI operation if not necessary.
            if (!isMasterNameValid)
            {
                nameInputField.text = validMasterName.ToString(0,
                    Mathf.Min(validMasterName.Length, gameSettings.MaxPlayerNameLength));
            }
        }

        private void ValidateInstanceName(string instanceName)
        {
            var validInstanceName = new StringBuilder(instanceName.Length);
            var isInstanceNameValid = instanceName.Length <= gameSettings.MinInstanceNameLength;
            foreach (var character in instanceName)
            {
                if (char.IsLetterOrDigit(character) || character == ' ' || character == '-' || character == '_' ||
                    character == '!' || character == '(' || character == ')')
                {
                    validInstanceName.Append(character);
                }
                else
                {
                    isInstanceNameValid = false;
                }
            }

            // Avoid expensive UI operation if not necessary.
            if (!isInstanceNameValid)
            {
                instanceNameInputField.text = validInstanceName.ToString(0,
                    Mathf.Min(validInstanceName.Length, gameSettings.MaxInstanceNameLength));
            }
        }

        private void ToggleButtons(bool value)
        {
            loginButton.UpdateButtonState(value
                ? ButtonState.Active
                : ButtonState.Inactive);

            createInstanceButton.UpdateButtonState(value
                ? ButtonState.Active
                : ButtonState.Inactive);

            clearSaveGamesButton.UpdateButtonState(value
                ? ButtonState.Active
                : ButtonState.Inactive);

            foreach (var customButton in instanceScrollView.GetComponentsInChildren<CustomButton>())
            {
                customButton.UpdateButtonState(value
                    ? ButtonState.Active
                    : ButtonState.Inactive);
            }
        }

        private void OnCreateSession()
        {
            ToggleButtons(false);

            if (string.IsNullOrWhiteSpace(instanceNameInputField.text))
            {
                OpenWarningDialog(LocalizationUtility.GetLocalizedString("instanceNameMissingWarning"));
                ToggleButtons(true);
            }
            else if (instanceNameInputField.text.Length > gameSettings.MaxInstanceNameLength)
            {
                OpenWarningDialog(LocalizationUtility.GetLocalizedString("instanceNameTooLongWarning",
                    new[] { gameSettings.MaxInstanceNameLength }));
                ToggleButtons(true);
            }
            else if (instanceNameInputField.text.Length < gameSettings.MinInstanceNameLength)
            {
                OpenWarningDialog(LocalizationUtility.GetLocalizedString("instanceNameTooShortWarning",
                    new[] { gameSettings.MinInstanceNameLength }));

                ToggleButtons(true);
            }
            else if (string.IsNullOrWhiteSpace(nameInputField.text))
            {
                OpenWarningDialog(LocalizationUtility.GetLocalizedString("playerNameMissingWarning"));
                ToggleButtons(true);
            }
            else if (nameInputField.text.Length > gameSettings.MaxPlayerNameLength)
            {
                OpenWarningDialog(LocalizationUtility.GetLocalizedString("playerNameTooLongWarning",
                    new[] { gameSettings.MaxPlayerNameLength }));
                ToggleButtons(true);
            }
            else if (nameInputField.text.Length < gameSettings.MinPlayerNameLength)
            {
                OpenWarningDialog(LocalizationUtility.GetLocalizedString("playerNameTooShortWarning",
                    new[] { gameSettings.MinPlayerNameLength }));
                ToggleButtons(true);
            }
            else
            {
                adminNameWhenStarted = nameInputField.text;
                instanceNameWhenStarted = instanceNameInputField.text;

                lobbySizeWhenStarted = (int)lobbySizeSliderInputField.Value;
                daylightDurationWhenStarted = (int)durationDaySliderInputField.Value;

                relaxCooldownInSecondsWhenStarted = relaxCooldownSliderInputField.Value;

                StartCoroutine(CreateBackendInstance(instanceNameWhenStarted));
            }
        }

        private void RefreshSaveGameData()
        {
            StartCoroutine(BackendConnection.ListAllInstances(HandleListAllInstances));
        }

        private void HandleListAllInstances(ParameterizedResponse<List<string>> response)
        {
            var list = LoadSave.RefreshSaveGameData(response.Data);
            foreach (Transform child in instanceScrollView)
            {
                Destroy(child.gameObject);
            }

            if (list == null || list.Count == 0)
            {
                return;
            }

            foreach (var saveGame in list)
            {
                var instanceButton = Instantiate(instanceButtonPrefab, instanceScrollView);
                instanceButton.SetupInstanceButton(saveGame.instanceUuid, () => OnLoadInstance(saveGame));
            }
        }

        private void OnLoadInstance(MasterSaveGame saveGame)
        {
            var roomOptions = PunManager.DefaultRoomOptions;
            roomOptions.MaxPlayers = byte.Parse(saveGame.lobbySize.ToString());
            roomOptions.CustomRoomProperties = new Hashtable();
            roomOptions.CustomRoomProperties.Merge(PunManager.DefaultRoomProperties);
            roomOptions.CustomRoomProperties.Merge(new Hashtable
            {
                { "daylightDuration", saveGame.daylightDuration },
                { "relaxCooldownInSeconds", saveGame.relaxCooldownInSeconds },
                { "gameRound", saveGame.gameRound },
                { "dayState", saveGame.dayState },
                { "daytimeProgress", saveGame.daytimeProgress }
            });

            if (mainMenu.punManager.HostGame(saveGame.instanceUuid, roomOptions))
            {
                BackendConnection.InstanceName = saveGame.instanceUuid;

                SentrySdk.ConfigureScope(scope =>
                {
                    scope.User = new User
                    {
                        Username = saveGame.playerName,
                        Other = new Dictionary<string, string>
                        {
                            { "PlayerID", "MasterClient" },
                            { "InstanceName", BackendConnection.InstanceName },
                            { "BackendUrl", BackendConnection.Url },
                            { "ProductName", Application.productName },
                            { "Version", Application.version }
                        }
                    };
                });

                mainMenu.SetLobbyRejoining(true);
                mainMenu.SwitchToLobbyMode();
                mainMenu.punManager.SetPlayerName(saveGame.playerName);
            }
            else
            {
                OpenErrorDialog(LocalizationUtility.GetLocalizedString("couldNotCreateRoomError"));
            }
        }

        private void OnLogin()
        {
            ToggleButtons(false);
            StartCoroutine(CheckMasterPassword(passwordInputField.text));
        }

        private IEnumerator CheckMasterPassword(string password)
        {
            var loadingDialog = canvasHandler.InitializeLoadingDialog(true);
            yield return BackendConnection.Login(response =>
            {
                switch (response.GetResponseType())
                {
                    case ResponseType.Success:
                        if (response.Data != null)
                        {
                            BackendConnection.LoginAsMasterClient(response.Data.ID, response.Data.Token);

                            loginView.SetActive(false);
                            createInstanceView.SetActive(true);
                            loadInstanceView.SetActive(true);
                        }
                        else
                        {
                            OpenWarningDialog(LocalizationUtility.GetLocalizedString("wrongMasterPasswordWarning"));
                        }

                        break;

                    case ResponseType.ServerError:
                        OpenErrorDialog(LocalizationUtility.GetLocalizedString("backendError", "contactAdmin"));
                        break;

                    case ResponseType.ClientError:
                        OpenWarningDialog(response.GetLocalizedResponse());
                        break;

                    default:
                        throw new UnityException($"Unknown response type {response.GetResponseType()}!");
                }

                Destroy(loadingDialog.gameObject);
                ToggleButtons(true);
            }, BackendConnection.MasterClientName, password, true);
        }

        private void OnClearSaveGames()
        {
            LoadSave.ClearMasterSaveGames();
            foreach (Transform child in instanceScrollView)
            {
                Destroy(child.gameObject);
            }
        }

        private IEnumerator CreateBackendInstance(string instanceName)
        {
            var loadingDialog = canvasHandler.InitializeLoadingDialog(true);
            yield return BackendConnection.CreateInstance(response =>
            {
                switch (response.GetResponseType())
                {
                    case ResponseType.Success:
                        if (response.TryGetResponseAsBool(out var value))
                        {
                            if (value)
                            {
                                BackendConnection.InstanceName = instanceName;
                                HostGame();
                            }
                            else
                            {
                                throw new UnityException(
                                    "What does no success mean here? No documentation in backend.");
                            }
                        }
                        else
                        {
                            OpenWarningDialog(LocalizationUtility.GetLocalizedString("instanceAlreadyExistsWarning"));
                        }

                        break;

                    case ResponseType.ServerError:
                        OpenErrorDialog(LocalizationUtility.GetLocalizedString("backendError", "contactAdmin"));
                        break;

                    case ResponseType.ClientError:
                        OpenWarningDialog(response.GetLocalizedResponse());
                        break;

                    default:
                        throw new UnityException($"Unknown response type {response.GetResponseType()}!");
                }

                Destroy(loadingDialog.gameObject);
                ToggleButtons(true);
            }, instanceName, true);
        }

        private void HostGame()
        {
            var roomOptions = PunManager.DefaultRoomOptions;
            roomOptions.MaxPlayers = byte.Parse(lobbySizeWhenStarted.ToString());
            roomOptions.CustomRoomProperties = new Hashtable();
            roomOptions.CustomRoomProperties.Merge(PunManager.DefaultRoomProperties);
            roomOptions.CustomRoomProperties.Merge(new Hashtable
            {
                { "daylightDuration", daylightDurationWhenStarted },
                { "relaxCooldownInSeconds", relaxCooldownInSecondsWhenStarted }
            });

            if (mainMenu.punManager.HostGame(instanceNameWhenStarted, roomOptions))
            {
                SentrySdk.ConfigureScope(scope =>
                {
                    scope.User = new User
                    {
                        Username = adminNameWhenStarted,
                        Other = new Dictionary<string, string>
                        {
                            { "PlayerID", "MasterClient" },
                            { "InstanceName", BackendConnection.InstanceName },
                            { "BackendUrl", BackendConnection.Url },
                            { "ProductName", Application.productName },
                            { "Version", Application.version }
                        }
                    };
                });

                LoadSave.AddOrUpdateMasterSaveGame(new MasterSaveGame(instanceNameWhenStarted, adminNameWhenStarted,
                    lobbySizeWhenStarted, roomOptions.CustomRoomProperties));
                mainMenu.SetLobbyRejoining(false);
                mainMenu.SwitchToLobbyMode();
                mainMenu.punManager.SetPlayerName(adminNameWhenStarted);
            }
            else
            {
                OpenErrorDialog(LocalizationUtility.GetLocalizedString("couldNotCreateRoomError"));
            }
        }

        private void OpenWarningDialog(string messageText)
        {
            mainMenu.GetComponent<CanvasGroup>().interactable = false;

            canvasHandler.InitializeOkDialog(LocalizationUtility.GetLocalizedString("warningTitle"), messageText,
                LocalizationUtility.GetLocalizedString("ok"), FeedbackType.Negative,
                () => mainMenu.GetComponent<CanvasGroup>().interactable = true);
        }

        private void OpenErrorDialog(string messageText)
        {
            mainMenu.GetComponent<CanvasGroup>().interactable = false;

            canvasHandler.InitializeOkDialog(LocalizationUtility.GetLocalizedString("criticalErrorTitle"), messageText,
                LocalizationUtility.GetLocalizedString("ok"), FeedbackType.Negative,
                () => mainMenu.GetComponent<CanvasGroup>().interactable = true);
        }
    }
}