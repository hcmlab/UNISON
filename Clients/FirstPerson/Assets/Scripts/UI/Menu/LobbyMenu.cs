using System;
using System.Collections;
using System.Linq;
using Backend;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using PUN;
using TMPro;
using UI.Auxiliary;
using UI.Dialogs;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace UI.Menu
{
    public class LobbyMenu : MonoBehaviour, IInRoomCallbacks
    {
        [SerializeField] private CanvasHandler canvasHandler;
        [SerializeField] private MainMenu mainMenu;
        [SerializeField] private TMP_Text instanceNameText;
        [SerializeField] private GameObject playerScrollViewContent;
        [SerializeField] private CustomButton startButton;
        [SerializeField] private CustomButton leaveButton;
        [SerializeField] private GameObject WaitForMasterText;
        [SerializeField] private TMP_Text pleaseWaitForMasterText;
        [SerializeField] private PlayerItem playerItemPrefab;

        [NonSerialized] public bool Rejoining;

        private bool playerListInitialized;

        private void Awake()
        {
            pleaseWaitForMasterText.gameObject.SetActive(false);

            startButton.OnClick.AddListener(OnStart);
            leaveButton.OnClick.AddListener(OnLeave);

            ToggleButtons(true);
        }

        private void Update()
        {
            if (PhotonNetwork.CurrentRoom != null)
            {
                instanceNameText.text = PhotonNetwork.CurrentRoom.Name;
            }

            // TODO: Optimize with subscriber
            if (PhotonNetwork.IsMasterClient)
            {
                startButton.gameObject.SetActive(!mainMenu.punManager.GameStarted);
                WaitForMasterText.SetActive(false);
            }
            else
            {
                startButton.gameObject.SetActive(false);
                WaitForMasterText.SetActive(true);
            }

            if (!playerListInitialized && PhotonNetwork.IsConnectedAndReady)
            {
                InitPlayerItems();
                playerListInitialized = true;
            }
        }

        private void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
            playerListInitialized = false;
        }

        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
            playerListInitialized = true;

            startButton.gameObject.SetActive(false);
        }

        private void OnLeave()
        {
            SaveCamera();
            ToggleButtons(false);
            mainMenu.SwitchToNormalMode();
            mainMenu.punManager.LeaveGame();
            ToggleButtons(true);
        }

        private void SaveCamera()
        {
            Debug.Log((GameObject.Find("PlayerManager(Clone)") != null) + " " + GameObject.Find("PlayerManager(Clone)").GetComponent<Clients.PlayerManager>().PunClient.IsMine);
            if (GameObject.Find("PlayerManager(Clone)") != null && GameObject.Find("PlayerManager(Clone)").GetComponent<Clients.PlayerManager>().PunClient.IsMine)
            {
                GameObject.Find("MainCamera").transform.SetParent(GameObject.Find("Focus").transform);
            }
        }

        private void OnStart()
        {
            ToggleButtons(false);
            if (Rejoining)
            {
                StartGame();
            }
            else
            {
                StartCoroutine(InitializeGame());
            }
        }

        private void ToggleButtons(bool value)
        {
            startButton.UpdateButtonState(value
                ? ButtonState.Active
                : ButtonState.Inactive);

            leaveButton.UpdateButtonState(value
                ? ButtonState.Active
                : ButtonState.Inactive);
        }

        public void OnPlayerEnteredRoom(Player player)
        {
            Debug.Log($"Player {player.NickName} joined this lobby.");
            InitPlayerItems();
        }

        public void OnPlayerLeftRoom(Player player)
        {
            Debug.Log($"Player {player.NickName} left this lobby.");
            InitPlayerItems();
        }

        public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
        }

        public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
        }

        public void OnMasterClientSwitched(Player newMasterClient)
        {
            mainMenu.SwitchToNormalMode();
        }

        private void InitPlayerItems()
        {
            foreach (Transform child in playerScrollViewContent.transform)
            {
                Destroy(child.gameObject);
            }

            if (!PhotonNetwork.IsMasterClient)
            {
                InstantiatePlayerItem(PhotonNetwork.MasterClient);
            }

            InstantiatePlayerItem(PhotonNetwork.LocalPlayer);

            foreach (Player player in PhotonNetwork.CurrentRoom.Players.Values.Where(player =>
                         !player.IsMasterClient && !player.IsLocal))
            {
                InstantiatePlayerItem(player);
            }

            UpdateStartButtonText();
        }

        private void InstantiatePlayerItem(Player player)
        {
            PlayerItem playerItem = Instantiate(playerItemPrefab, playerScrollViewContent.transform);
            playerItem.Setup(player);
        }

        private void UpdateStartButtonText()
        {
            if (PhotonNetwork.CurrentRoom == null)
            {
                return;
            }

            startButton.GetComponentInChildren<TMP_Text>().text =
                "Start (" + PhotonNetwork.CurrentRoom.PlayerCount + "/" +
                PhotonNetwork.CurrentRoom.MaxPlayers + ")";
        }

        private IEnumerator InitializeGame()
        {
            LoadingDialog loadingDialog = canvasHandler.InitializeLoadingDialog(true);
            yield return BackendConnection.Initialize(response =>
            {
                switch (response.GetResponseType())
                {
                    case ResponseType.Success:
                        if (response.TryGetResponseAsBool(out bool value))
                        {
                            if (value)
                            {
                                StartGame();
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
                        ToggleButtons(true);
                        break;

                    case ResponseType.ClientError:
                        OpenWarningDialog(response.GetLocalizedResponse());
                        ToggleButtons(true);
                        break;

                    default:
                        throw new UnityException($"Unknown response type {response.GetResponseType()}!");
                }

                Destroy(loadingDialog.gameObject);
            });
        }

        private void StartGame()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                OpenWarningDialog(LocalizationUtility.GetLocalizedString("onlyMasterClientError"));
                ToggleButtons(true);
                return;
            }

            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent(PunManager.StartGameEventCode, null, raiseEventOptions, SendOptions.SendReliable);

            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable()
            {
                { "gameStarted", true }
            });

            ToggleButtons(true);
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