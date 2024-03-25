using System.Collections;
using System.Collections.Generic;
using Backend;
using Photon.Pun;
using Photon.Realtime;
using Sentry;
using Settings;
using TMPro;
using UI.Auxiliary;
using UI.Dialogs;
using UnityEngine;

namespace UI.Menu
{
    public class GeneralMenu : MonoBehaviour, ILobbyCallbacks
    {
        [SerializeField] private CanvasHandler canvasHandler;
        [SerializeField] private MainMenu mainMenu;
        [SerializeField] private TMP_InputField nameInputField;
        [SerializeField] private Transform instanceScrollView;
        [SerializeField] private CustomButton rejoinButton;
        [SerializeField] private TMP_Text noInstanceAvailableText;
        [SerializeField] private InstanceButton instanceButtonPrefab;
        [SerializeField] private GameSettings gameSettings;

        private Dictionary<string, RoomInfo> roomDictionary;

        private void Awake()
        {
            rejoinButton.OnClick.AddListener(OnRejoin);
            roomDictionary = new Dictionary<string, RoomInfo>();

            ToggleButtons(true);
        }

        private void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);

            StartCoroutine(JoinLobby());
            StartCoroutine(RefreshRejoin());
        }

        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
            if (PhotonNetwork.InLobby)
            {
                PhotonNetwork.LeaveLobby();
            }

            StopCoroutine(RefreshRejoin());
            StopCoroutine(JoinLobby());
        }

        public void OnRoomListUpdate(List<RoomInfo> updatedRooms)
        {
            foreach (var roomInfo in updatedRooms)
            {
                if (roomInfo.RemovedFromList)
                {
                    roomDictionary.Remove(roomInfo.Name);
                }
                else
                {
                    roomDictionary[roomInfo.Name] = roomInfo;
                }
            }

            noInstanceAvailableText.gameObject.SetActive(roomDictionary.Count == 0);
            if (updatedRooms.Count <= 0)
            {
                return;
            }

            foreach (Transform child in instanceScrollView)
            {
                Destroy(child.gameObject);
            }

            foreach (var room in roomDictionary)
            {
                var instanceButton = Instantiate(instanceButtonPrefab, instanceScrollView);

                instanceButton.SetupInstanceButton(room.Key, room.Value.PlayerCount, room.Value.MaxPlayers,
                    () => OnJoin(room.Value.Name, nameInputField.text));
            }
        }

        public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
        {
        }

        public void OnJoinedLobby()
        {
            roomDictionary.Clear();
            foreach (Transform child in instanceScrollView)
            {
                Destroy(child.gameObject);
            }
        }

        public void OnLeftLobby()
        {
            roomDictionary.Clear();
            foreach (Transform child in instanceScrollView)
            {
                Destroy(child.gameObject);
            }
        }

        private IEnumerator JoinLobby()
        {
            var joining = false;
            while (isActiveAndEnabled)
            {
                if (PhotonNetwork.IsConnectedAndReady && !PhotonNetwork.InLobby && !joining &&
                    PhotonNetwork.Server == ServerConnection.MasterServer)
                {
                    PhotonNetwork.JoinLobby();
                    joining = true;
                }

                if (PhotonNetwork.InLobby)
                {
                    StopCoroutine(JoinLobby());
                }

                yield return new WaitForSeconds(gameSettings.RefreshTime);
            }
        }

        private void ToggleButtons(bool value)
        {
            rejoinButton.UpdateButtonState(value
                ? ButtonState.Active
                : ButtonState.Inactive);

            foreach (var customButton in instanceScrollView.GetComponentsInChildren<CustomButton>())
            {
                customButton.UpdateButtonState(value
                    ? ButtonState.Active
                    : ButtonState.Inactive);
            }
        }

        private IEnumerator RefreshRejoin()
        {
            while (isActiveAndEnabled)
            {
                rejoinButton.gameObject.SetActive(true);
                var playerSaveGame = LoadSave.LoadPlayerSaveGameFromFile();
                if (playerSaveGame == null || roomDictionary == null)
                {
                    rejoinButton.gameObject.SetActive(false);
                }
                else if (!roomDictionary.ContainsKey(playerSaveGame.InstanceUuid))
                {
                    rejoinButton.gameObject.SetActive(false);
                }

                yield return new WaitForSeconds(gameSettings.RefreshTime);
            }
        }

        private void OnRejoin()
        {
            OnRejoin(null);
        }

        private void OnRejoin(PlayerSaveGame playerSaveGame)
        {
            ToggleButtons(false);

            playerSaveGame ??= LoadSave.LoadPlayerSaveGameFromFile();
            if (playerSaveGame == null)
            {
                OpenWarningDialog(LocalizationUtility.GetLocalizedString("rejoinNotAvailableWarning"));
                ToggleButtons(true);
                return;
            }

            if (!roomDictionary.ContainsKey(playerSaveGame.InstanceUuid))
            {
                OpenWarningDialog(LocalizationUtility.GetLocalizedString("rejoinNotAvailableWarning"));
                ToggleButtons(true);
                return;
            }

            if (!PhotonNetwork.IsConnectedAndReady)
            {
                OpenErrorDialog(LocalizationUtility.GetLocalizedString("photonConnectionError", "contactAdmin"));
                ToggleButtons(true);
                return;
            }

            BackendConnection.InstanceName = playerSaveGame.InstanceUuid;

            SentrySdk.ConfigureScope(scope =>
            {
                scope.User = new User
                {
                    Username = nameInputField.text,
                    Other = new Dictionary<string, string>
                    {
                        {
                            "PlayerID",
                            BackendConnection.PlayerID.HasValue
                                ? BackendConnection.PlayerID.Value.ToString()
                                : "No player ID"
                        },
                        { "InstanceName", BackendConnection.InstanceName },
                        { "BackendUrl", BackendConnection.Url },
                        { "ProductName", Application.productName },
                        { "Version", Application.version }
                    }
                };
            });

            PhotonNetwork.JoinRoom(BackendConnection.InstanceName);
            mainMenu.punManager.SetPlayerName(nameInputField.text);
            mainMenu.SwitchToLobbyMode();

            ToggleButtons(true);
        }

        private void OnJoin(string instance, string playerName)
        {
            ToggleButtons(false);

            if (roomDictionary[instance].PlayerCount >= roomDictionary[instance].MaxPlayers)
            {
                OpenWarningDialog(LocalizationUtility.GetLocalizedString("roomFullWarning"));
                ToggleButtons(true);
            }
            else
            {
                var playerSaveGame = LoadSave.LoadPlayerSaveGameFromFile();
                if (playerSaveGame != null &&
                    playerSaveGame.InstanceUuid == instance &&
                    playerSaveGame.PlayerName == playerName &&
                    BackendConnection.PlayerID.HasValue &&
                    playerSaveGame.PlayerID == BackendConnection.PlayerID.Value)
                {
                    OnRejoin(playerSaveGame);
                }
                else
                {
                    BackendConnection.InstanceName = instance;
                    StartCoroutine(RegisterPlayerForInstance(playerName));
                }
            }
        }

        private IEnumerator RegisterPlayerForInstance(string playerName)
        {
            var loadingDialog = canvasHandler.InitializeLoadingDialog(true);

            yield return BackendConnection.RegisterPlayerForInstance(response =>
            {
                switch (response.GetResponseType())
                {
                    case ResponseType.Success:
                        if (response.TryGetResponseAsBool(out var value))
                        {
                            if (value)
                            {
                                if (!PhotonNetwork.IsConnectedAndReady)
                                {
                                    OpenErrorDialog(LocalizationUtility.GetLocalizedString("photonConnectionError",
                                        "contactAdmin"));
                                }
                                else
                                {
                                    SentrySdk.ConfigureScope(scope =>
                                    {
                                        scope.User = new User
                                        {
                                            Username = playerName,
                                            Other = new Dictionary<string, string>
                                            {
                                                {
                                                    "PlayerID",
                                                    BackendConnection.PlayerID.HasValue
                                                        ? BackendConnection.PlayerID.Value.ToString()
                                                        : "No player ID"
                                                },
                                                { "InstanceName", BackendConnection.InstanceName },
                                                { "BackendUrl", BackendConnection.Url },
                                                { "ProductName", Application.productName },
                                                { "Version", Application.version }
                                            }
                                        };
                                    });

                                    PhotonNetwork.JoinRoom(BackendConnection.InstanceName);
                                    mainMenu.punManager.SetPlayerName(playerName);
                                    mainMenu.SwitchToLobbyMode();
                                    LoadSave.SavePlayerSaveGameToFile(new PlayerSaveGame(BackendConnection.InstanceName,
                                        BackendConnection.PlayerID.HasValue ? BackendConnection.PlayerID.Value : -1,
                                        playerName));
                                }
                            }
                            else
                            {
                                throw new UnityException(
                                    "What does no success mean here? No documentation in backend.");
                            }
                        }
                        else
                        {
                            throw new UnityException($"Response \"{response.GetRawResponse()}\" not a bool!");
                        }

                        break;

                    case ResponseType.ServerError:
                        OpenErrorDialog(LocalizationUtility.GetLocalizedString("backendError", "contactAdmin"));
                        break;

                    case ResponseType.ClientError:
                        OpenErrorDialog(response.GetLocalizedResponse());
                        break;

                    default:
                        throw new UnityException($"Unknown response type {response.GetResponseType()}!");
                }

                Destroy(loadingDialog.gameObject);
                ToggleButtons(true);
            });
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