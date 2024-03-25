using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Backend;
using Backend.ResponseDataCapsules;
using JetBrains.Annotations;
using Settings;
using TMPro;
using UI.Auxiliary;
using UnityEngine;
using UnityEngine.UI;
using World;

namespace UI.HUD
{
    /// <summary>
    ///     The master can see the stats of all players in this view
    /// </summary>
    public class PlayerStatsHUD : ScrollingHUD, IStateSubscriber<DayState>
    {
        private const int UpdatePlayerStatsContent = 1;
        private const int UpdatePetitionEffectsContent = 2;
        private const int UpdateOpenPetitionsContent = 3;
        private const int UpdateOtherStatsContent = 4;

        private static readonly Dictionary<int, DateTime> StartedCreativeByPlayerIDs = new Dictionary<int, DateTime>();
        [SerializeField] private CanvasGroup canvasGroup;

        [SerializeField] private Button openPlayerStatsButton;
        [SerializeField] private Button openPetitionEffectsButton;
        [SerializeField] private Button openOpenPetitionsButton;
        [SerializeField] private Button openOtherStatsButton;

        [SerializeField] private Button endEveningButton;

        [SerializeField] private GameObject playerStatsContent;
        [SerializeField] private Transform playerStatsContentTransform;

        [SerializeField] private GameObject petitionEffectsContent;
        [SerializeField] private TMP_Text petitionEffectsText;

        [SerializeField] private GameObject openPetitionsContent;
        [SerializeField] private Transform openPetitionsContentTransform;

        [SerializeField] private GameObject otherStatsContent;
        [SerializeField] private TMP_Text otherStatsText;

        [SerializeField] private PlayerStatsListTile playerStatsListTilePrefab;
        [SerializeField] private GameObject templateItemPrefab;
        [SerializeField] private PetitionStatsItem openPetitionItemPrefab;
        [SerializeField] private PetitionStatsItem closedPetitionItemPrefab;

        private readonly Dictionary<int, string> playerNameByPlayerIDs = new Dictionary<int, string>();
        private IEnumerator checkEndEveningCoroutine;
        private IEnumerator updateOpenPetitionsCoroutine;
        private IEnumerator updateOtherStatsCoroutine;
        private IEnumerator updatePetitionEffectsCoroutine;

        private IEnumerator updatePlayerStatsCoroutine;

        private void Start()
        {
            openPlayerStatsButton.onClick.RemoveAllListeners();
            openPlayerStatsButton.onClick.AddListener(OpenPlayerStats);

            openPetitionEffectsButton.onClick.RemoveAllListeners();
            openPetitionEffectsButton.onClick.AddListener(OpenPetitionEffects);

            openOpenPetitionsButton.onClick.RemoveAllListeners();
            openOpenPetitionsButton.onClick.AddListener(OpenOpenPetitions);

            openOtherStatsButton.onClick.RemoveAllListeners();
            openOtherStatsButton.onClick.AddListener(OpenOtherStats);

            endEveningButton.onClick.RemoveAllListeners();
            endEveningButton.onClick.AddListener(EndEvening);

            ToggleHUD(false);

            updatePlayerStatsCoroutine = UpdatePlayerStats();
            updatePetitionEffectsCoroutine = UpdatePetitionEffects();
            updateOpenPetitionsCoroutine = UpdateOpenPetitions();
            updateOtherStatsCoroutine = UpdateOtherStats();
            checkEndEveningCoroutine = CheckEndEvening();
        }

        private void Update()
        {
            if (Input.GetKeyDown(ControlSettings.ControlData.ToggleStats))
            {
                if (canvasGroup.interactable)
                {
                    ToggleHUD(false);
                }
                else
                {
                    ToggleHUD(true);
                    OpenPlayerStats();
                }
            }
        }

        public void OnUpdated(DayState newDayState, DayState oldDayState)
        {
            switch (newDayState)
            {
                case DayState.Daylight:
                case DayState.Evening:
                    StartIsCreativeAtDayStart();
                    break;
                case DayState.Dusk:
                case DayState.NextDay:
                    StopIsCreateAtDayEnd();
                    break;
            }
        }

        public override void OnBeingEnabled(WorldManager worldManager)
        {
            base.OnBeingEnabled(worldManager);
            CurrentWorldManager.SubscribeToDayState(this);
        }

        public override void OnBeingDisabled()
        {
            base.OnBeingDisabled();
            CurrentWorldManager.UnsubscribeFromDayState(this);
        }

        private void StopIsCreateAtDayEnd()
        {
            Debug.Log($"Stopping creative for all players");
            var playerIDs = StartedCreativeByPlayerIDs.Keys;

            foreach (var playerID in playerIDs)
            {
                StopIsCreative(playerID);
            }
        }

        private void StartIsCreativeAtDayStart()
        {
            Debug.Log("Starting creative for all players");
            StartCoroutine(BackendConnection.GetPlayers(response =>
            {
                if (response.IsSuccess())
                {
                    foreach (var player in response.Data)
                    {
                        if (StartedCreativeByPlayerIDs.Keys.Contains(player.ID))
                        {
                            continue;
                        }

                        StartIsCreative(player.ID);
                    }
                }
                else
                {
                    Debug.LogError($"Error when starting creative for all players: \"{response.GetRawResponse()}\"");
                }
            }));
        }

        private void OpenPlayerStats()
        {
            ToggleContent(UpdatePlayerStatsContent);
        }

        private IEnumerator UpdatePlayerStats()
        {
            while (isActiveAndEnabled)
            {
                yield return BackendConnection.GetPlayers(response =>
                {
                    if (response.IsSuccess())
                    {
                        foreach (Transform child in playerStatsContentTransform)
                        {
                            Destroy(child.gameObject);
                        }

                        var players = response.Data;
                        if (players != null)
                        {
                            foreach (var player in players)
                            {
                                var playerStatsListTile = Instantiate(playerStatsListTilePrefab,
                                    playerStatsContentTransform);
                                var isCreative = StartedCreativeByPlayerIDs.ContainsKey(player.ID);
                                playerStatsListTile.SetPlayerProperties(player, isCreative, StartIsCreative,
                                    StopIsCreative);
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError($"Error when updating player stats HUD: \"{response.GetRawResponse()}\"");
                    }
                });

                yield return new WaitForSeconds(1f);
            }
        }

        private void StartIsCreative(int creativePlayerID)
        {
            if (StartedCreativeByPlayerIDs.Remove(creativePlayerID))
            {
                Debug.LogError($"Had to remove previous started creative time for player #{creativePlayerID}");
            }

            Debug.Log($"Started creative for player #{creativePlayerID}");
            StartedCreativeByPlayerIDs[creativePlayerID] = DateTime.Now;
        }

        private void StopIsCreative(int creativePlayerID, [CanBeNull] Action onDone = null)
        {
            if (StartedCreativeByPlayerIDs.TryGetValue(creativePlayerID, out var startedCreative))
            {
                StartCoroutine(BackendConnection.SetCreativeTime(response =>
                {
                    StartedCreativeByPlayerIDs.Remove(creativePlayerID);

                    if (response.IsSuccess())
                    {
                        Debug.Log($"Stopped creative for player #{creativePlayerID}");
                    }
                    else
                    {
                        Debug.LogError($"Could not stop: \"{response.GetRawResponse()}\"");
                    }

                    onDone?.Invoke();
                }, creativePlayerID, startedCreative, DateTime.Now, true));
            }
            else
            {
                Debug.LogError($"No started creative time for player #{creativePlayerID}");
            }
        }

        private void OpenPetitionEffects()
        {
            ToggleContent(UpdatePetitionEffectsContent);
        }

        private IEnumerator UpdatePetitionEffects()
        {
            while (isActiveAndEnabled)
            {
                Dictionary<Scale, int> scaleValues = null;
                yield return BackendConnection.GetScales(response =>
                {
                    if (response.IsSuccess())
                    {
                        scaleValues = response.Data;
                    }
                    else
                    {
                        Debug.LogError($"Error when updating player stats HUD: \"{response.GetRawResponse()}\"");
                    }
                }, DialogUtility.PetitionScales);
                if (scaleValues == null)
                {
                    yield break;
                }

                List<Petition> petitions = null;
                yield return BackendConnection.GetPetitions(response =>
                {
                    if (response.IsSuccess())
                    {
                        petitions = response.Data;
                    }
                    else
                    {
                        Debug.LogError($"Error when updating player stats HUD: \"{response.GetRawResponse()}\"");
                    }
                });
                if (petitions == null)
                {
                    yield break;
                }

                petitionEffectsText.text = DialogUtility.CreatePetitionEffectsText(scaleValues, petitions);

                yield return new WaitForSeconds(1f);
            }
        }

        private void OpenOpenPetitions()
        {
            ToggleContent(UpdateOpenPetitionsContent);
        }

        private IEnumerator UpdateOpenPetitions()
        {
            while (isActiveAndEnabled)
            {
                List<Template> templates = null;
                yield return BackendConnection.ListAllPetitionTemplates(response =>
                {
                    if (response.IsSuccess())
                    {
                        templates = response.Data;
                    }
                    else
                    {
                        Debug.LogError($"Error when updating player stats HUD: \"{response.GetRawResponse()}\"");
                    }
                });
                if (templates == null)
                {
                    yield break;
                }

                List<Petition> petitions = null;
                yield return BackendConnection.GetPetitions(response =>
                {
                    if (response.IsSuccess())
                    {
                        petitions = response.Data;
                    }
                    else
                    {
                        Debug.LogError($"Error when updating player stats HUD: \"{response.GetRawResponse()}\"");
                    }
                });
                if (petitions == null)
                {
                    yield break;
                }

                yield return BackendConnection.GetPlayers(response =>
                {
                    if (response.IsSuccess())
                    {
                        foreach (var player in response.Data)
                        {
                            playerNameByPlayerIDs[player.ID] = player.Name;
                        }
                    }
                    else
                    {
                        Debug.LogError($"Error when updating player stats HUD: \"{response.GetRawResponse()}\"");
                    }
                });

                foreach (Transform child in openPetitionsContentTransform)
                {
                    Destroy(child.gameObject);
                }

                foreach (var template in templates)
                {
                    var templateItem = Instantiate(templateItemPrefab, openPetitionsContentTransform);
                    templateItem.GetComponentInChildren<TMP_Text>().text = DialogUtility.CreatePetitionText(template);

                    var openPetitions = new List<Petition>();
                    var closedPetitions = new List<Petition>();

                    foreach (var petition in petitions)
                    {
                        if (template.PositiveText == petition.PositiveText &&
                            template.NegativeText == petition.NegativeText)
                        {
                            switch (petition.Status)
                            {
                                case "open":
                                    openPetitions.Add(petition);
                                    break;

                                case "closed":
                                    closedPetitions.Add(petition);
                                    break;
                            }
                        }
                    }

                    foreach (var openPetition in openPetitions)
                    {
                        var petitionItem =
                            Instantiate(openPetitionItemPrefab, openPetitionsContentTransform);
                        petitionItem.summary.text = DialogUtility.CreatePetitionText(openPetition);

                        petitionItem.numberPlayerYesVotes.text = "+: " + openPetition.YesVotes.Count + "/" + playerNameByPlayerIDs.Count;

                        petitionItem.numberPlayerNoVotes.text = "-: " + openPetition.NoVotes.Count + "/" + playerNameByPlayerIDs.Count;

                        /*petitionItem.numberPlayerYesVotes.text = LocalizationUtility.GetLocalizedString(
                            "numberYesVotes", new[] { openPetition.YesVotes.Count, playerNameByPlayerIDs.Count });

                        petitionItem.numberPlayerNoVotes.text = LocalizationUtility.GetLocalizedString(
                            "numberNoVotes", new[] { openPetition.NoVotes.Count, playerNameByPlayerIDs.Count }); */ 

                        petitionItem.playerYesVotes.ClearOptions();
                        petitionItem.playerYesVotes.AddOptions(GetPlayerNamesForIDs(openPetition.YesVotes));

                        petitionItem.playerNoVotes.ClearOptions();
                        petitionItem.playerNoVotes.AddOptions(GetPlayerNamesForIDs(openPetition.NoVotes));
                    }

                    foreach (var closedPetition in closedPetitions)
                    {
                        var petitionItem =
                            Instantiate(closedPetitionItemPrefab, openPetitionsContentTransform);
                        petitionItem.summary.text = DialogUtility.CreatePetitionText(closedPetition);

                        petitionItem.numberPlayerYesVotes.text = "+: " + closedPetition.YesVotes.Count + "/" + playerNameByPlayerIDs.Count;

                        petitionItem.numberPlayerNoVotes.text = "-: " + closedPetition.NoVotes.Count + "/" + playerNameByPlayerIDs.Count;

                        /*petitionItem.numberPlayerYesVotes.text = LocalizationUtility.GetLocalizedString(
                            "numberYesVotes", new[] { closedPetition.YesVotes.Count, playerNameByPlayerIDs.Count });

                        petitionItem.numberPlayerNoVotes.text = LocalizationUtility.GetLocalizedString(
                            "numberNoVotes", new[] { closedPetition.NoVotes.Count, playerNameByPlayerIDs.Count }); */

                        petitionItem.playerYesVotes.ClearOptions();
                        petitionItem.playerYesVotes.AddOptions(GetPlayerNamesForIDs(closedPetition.YesVotes));

                        petitionItem.playerNoVotes.ClearOptions();
                        petitionItem.playerNoVotes.AddOptions(GetPlayerNamesForIDs(closedPetition.NoVotes));
                    }
                }

                yield return new WaitForSeconds(1f);
            }
        }

        private void OpenOtherStats()
        {
            ToggleContent(UpdateOtherStatsContent);
        }

        private IEnumerator UpdateOtherStats()
        {
            while (isActiveAndEnabled)
            {
                Funds funds = null;
                yield return BackendConnection.GetGlobalFunds(response =>
                {
                    if (response.IsSuccess())
                    {
                        funds = response.Data;
                    }
                    else
                    {
                        Debug.LogError($"Error when updating player stats HUD: \"{response.GetRawResponse()}\"");
                    }
                });
                if (funds == null)
                {
                    yield break;
                }

                otherStatsText.text = LocalizationUtility.GetLocalizedString("globalFundsMessage", new[]
                {
                    funds.VaccinationFund.ToString("N2", CultureInfo.InvariantCulture),
                    funds.Stocks.ToString("N2", CultureInfo.InvariantCulture),
                    funds.TaxAmount.ToString("N2", CultureInfo.InvariantCulture)
                });

                yield return new WaitForSeconds(1f);
            }
        }

        private IEnumerator CheckEndEvening()
        {
            while (isActiveAndEnabled)
            {
                endEveningButton.interactable = CurrentWorldManager.DayState == DayState.Evening;

                yield return new WaitForSeconds(0.1f);
            }
        }

        private void EndEvening()
        {
            endEveningButton.interactable = false;
            CurrentWorldManager.MasterManager.IsEveningEnded = true;
            ToggleHUD(false);
        }

        private void ToggleHUD(bool toggle)
        {
            StopCoroutines();

            if (toggle)
            {
                if (checkEndEveningCoroutine != null)
                {
                    StopCoroutine(checkEndEveningCoroutine);
                }

                StartCoroutine(checkEndEveningCoroutine);
            }
            else if (checkEndEveningCoroutine != null)
            {
                StopCoroutine(checkEndEveningCoroutine);
            }

            canvasGroup.interactable = toggle;
            canvasGroup.blocksRaycasts = toggle;
            canvasGroup.alpha = toggle ? 1 : 0;
        }

        private void ToggleContent(int content)
        {
            StopCoroutines();

            switch (content)
            {
                case UpdatePlayerStatsContent:
                    StartCoroutine(updatePlayerStatsCoroutine);
                    break;

                case UpdatePetitionEffectsContent:
                    StartCoroutine(updatePetitionEffectsCoroutine);
                    break;

                case UpdateOpenPetitionsContent:
                    StartCoroutine(updateOpenPetitionsCoroutine);
                    break;

                case UpdateOtherStatsContent:
                    StartCoroutine(updateOtherStatsCoroutine);
                    break;
            }

            playerStatsContent.SetActive(content == UpdatePlayerStatsContent);
            petitionEffectsContent.SetActive(content == UpdatePetitionEffectsContent);
            openPetitionsContent.SetActive(content == UpdateOpenPetitionsContent);
            otherStatsContent.SetActive(content == UpdateOtherStatsContent);

            openPlayerStatsButton.interactable = content != UpdatePlayerStatsContent;
            openPetitionEffectsButton.interactable = content != UpdatePetitionEffectsContent;
            openOpenPetitionsButton.interactable = content != UpdateOpenPetitionsContent;
            openOtherStatsButton.interactable = content != UpdateOtherStatsContent;
        }

        private void StopCoroutines()
        {
            if (updatePlayerStatsCoroutine != null)
            {
                StopCoroutine(updatePlayerStatsCoroutine);
            }

            if (updatePetitionEffectsCoroutine != null)
            {
                StopCoroutine(updatePetitionEffectsCoroutine);
            }

            if (updateOpenPetitionsCoroutine != null)
            {
                StopCoroutine(updateOpenPetitionsCoroutine);
            }

            if (updateOtherStatsCoroutine != null)
            {
                StopCoroutine(updateOtherStatsCoroutine);
            }
        }

        private List<string> GetPlayerNamesForIDs(List<int> playerIDs)
        {
            var playerNames = new List<string>();
            foreach (var playerID in playerIDs)
            {
                playerNames.Add(playerNameByPlayerIDs.TryGetValue(playerID, out var playerName)
                    ? playerName
                    : LocalizationUtility.GetLocalizedString("unknownPlayer"));
            }

            return playerNames;
        }
    }
}

