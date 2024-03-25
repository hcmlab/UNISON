using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Backend;
using Backend.ResponseDataCapsules;
using PUN;
using UI.Auxiliary;
using UI.Dialogs;
using UnityEngine;

namespace Stations.Zones.TownHall
{
    public enum PetitionAction
    {
        Open,
        Close,
        Delete,
    }

    public class PetitionZone : InteractableWithDialog<PetitionDialog>
    {
        [SerializeField] private PetitionItem petitionItemPrefab;
        [SerializeField] private VoteItem voteItemPrefab;

        protected override ZoneType ZoneType => ZoneType.Petition;
        protected override string ActionSpriteName => "Petition";

        private YesNoDialog closePetitionDialog;
        private YesNoDialog deletePetitionDialog;

        private bool isInitialOpenPetitionList;

        private int? currentPetitionID;

        protected override void StartAction()
        {
            StartCooldown();

            isInitialOpenPetitionList = true;
            OpenPetitionList();
        }

        private void OpenPetitionList()
        {
            if (!isInitialOpenPetitionList)
            {
                CurrentDialog.ToggleButtons(false);
            }

            StartCoroutine(BackendConnection.GetPetitions(HandleOpenPetitionList, true));
        }

        private void HandleOpenPetitionList(ParameterizedResponse<List<Petition>> response)
        {
            if (HandlePossibleErrorResponse(response))
            {
                if (isInitialOpenPetitionList)
                {
                    CurrentDialog = WorldManager.canvasHandler.InitializePetitionDialog(
                        LocalizationUtility.GetLocalizedString("petitionTitle"),
                        LocalizationUtility.GetLocalizedString("petitionMessage"),
                        LocalizationUtility.GetLocalizedString("register"),
                        LocalizationUtility.GetLocalizedString("close"),
                        TryToOpenTemplateList,
                        OnCloseDialog
                    );

                    isInitialOpenPetitionList = false;
                }

                CurrentDialog.OpenPetitionList();
                AddDialogComponents(response.Data);
            }

            CurrentDialog.ToggleButtons(true);
        }

        private void AddDialogComponents(List<Petition> petitions)
        {
            // Clear all previous petitions.
            foreach (Transform child in CurrentDialog.GetPetitionsTransform())
            {
                Destroy(child.gameObject);
            }

            foreach (Petition petition in petitions)
            {
                if (petition.Status == "open")
                {
                    PetitionItem petitionItem = Instantiate(petitionItemPrefab, CurrentDialog.GetPetitionsTransform());

                    string petitionText = DialogUtility.CreatePetitionText(petition);
                    petitionItem.summary.text = petitionText;

                    int numberPlayersAlreadyVoted = petition.YesVotes.Count + petition.NoVotes.Count;
                    petitionItem.playersAlreadyVoted.text =
                        LocalizationUtility.GetLocalizedString("playersAlreadyVotedMessage",
                            new[] { numberPlayersAlreadyVoted });

                    petitionItem.closeButton.OnClick.AddListener(() =>
                        AskToClosePetition(petition.ID, petitionText, petitionItem.closeButton));
                    petitionItem.deleteButton.OnClick.AddListener(() =>
                        AskToDeletePetition(petition.ID, petitionText, petitionItem.deleteButton));
                }
            }
        }

        private void TryToOpenTemplateList()
        {
            CurrentDialog.ToggleButtons(false, CurrentDialog.yesButton);
            LockPetitionZone(
                id => { OpenTemplateList(); },
                id =>
                {
                    WorldManager.TryGetPlayerByActorNumber(id, out PunPlayer player);
                    if (player != null && player.Name != null)
                    {
                        ShowNotAllowedMessage(LocalizationUtility.GetLocalizedString("playerManagingPetitionsWarning",
                            new[] { player.Name }));
                    }
                    else
                    {
                        ShowNotAllowedMessage(
                            LocalizationUtility.GetLocalizedString("anotherPlayerManagingPetitionsWarning"));
                    }

                    CurrentDialog.ToggleButtons(true);
                });
        }

        private void TryToOpenPetitionList()
        {
            CurrentDialog.ToggleButtons(false, CurrentDialog.backToPetitionListButton);
            FreePetitionZone(null, null, OpenPetitionList);
        }

        private void OpenTemplateList()
        {
            CurrentDialog.ToggleButtons(false);
            StartCoroutine(StartOpeningTemplateList());
        }

        private IEnumerator StartOpeningTemplateList()
        {
            List<Template> templates = new List<Template>();
            yield return BackendConnection.ListAllPetitionTemplates(response =>
            {
                if (HandlePossibleErrorResponse(response))
                {
                    templates = response.Data;
                }
            }, true);

            CurrentDialog.OpenTemplateList();

            // Clear all previous templates.
            foreach (Transform child in CurrentDialog.GetTemplatesTransform())
            {
                Destroy(child.gameObject);
            }

            foreach (Template template in templates)
            {
                VoteItem templateButton = Instantiate(voteItemPrefab, CurrentDialog.GetTemplatesTransform());

                string templateText = DialogUtility.CreatePetitionText(template);
                templateButton.summary.text = templateText;

                if (template.HasOpenPetition)
                {
                    templateButton.button.Interactable = false;
                }
                else
                {
                    templateButton.button.Interactable = true;
                    templateButton.button.OnClick.AddListener(() =>
                    {
                        CurrentDialog.ToggleButtons(false, templateButton.button);
                        OpenPetitionTemplate(template, templateText);
                    });
                }
            }

            CurrentDialog.AddBackToPetitionListClickListener(TryToOpenPetitionList);
            CurrentDialog.ToggleButtons(true);
        }

        private void OpenPetitionTemplate(Template template, string templateText)
        {
            CurrentDialog.OpenPetitionTemplate(template.ValueType);
            CurrentDialog.UpdatePetitionTemplateContent(templateText,
                () => { AskToRegisterNewPetition(template); },
                OpenTemplateList);
            CurrentDialog.ToggleButtons(true);
        }

        private void LockPetitionZone(Action<int> onSuccess = null, Action<int> onFail = null,
            Action onFinally = null)
        {
            StartCoroutine(WorldManager.CompareAndSwapRoomProperty("petitionUser",
                -1, WorldManager.ClientManager.PunClient.ID,
                onSuccess, onFail, onFinally));
        }

        private void FreePetitionZone(Action<int> onSuccess = null, Action<int> onFail = null,
            Action onFinally = null)
        {
            StartCoroutine(WorldManager.CompareAndSwapRoomProperty("petitionUser",
                WorldManager.ClientManager.PunClient.ID, -1, -1,
                onSuccess, onFail, onFinally));
        }

        private void AskToRegisterNewPetition(Template template)
        {
            CurrentDialog.ToggleButtons(false, CurrentDialog.openPetitionButton);

            int petitionValue;

            switch (template.ValueType)
            {
                case "bool":
                    petitionValue = template.CurrentValue switch
                    {
                        0 => 1,
                        1 => 0,
                        _ => throw new UnityException($"Unknown bool value \"{template.CurrentValue}\"!")
                    };
                    break;

                case "int":
                    string possiblePetitionValue = CurrentDialog.GetPetitionValueText().Trim();

                    if (possiblePetitionValue == "")
                    {
                        ShowClientErrorMessage(LocalizationUtility.GetLocalizedString("missingPetitionValueWarning"));
                        CurrentDialog.ToggleButtons(true);
                        return;
                    }

                    if (!int.TryParse(possiblePetitionValue, NumberStyles.Integer, CultureInfo.InvariantCulture,
                            out petitionValue))
                    {
                        ShowClientErrorMessage(LocalizationUtility.GetLocalizedString("missingPetitionNumberWarning"));
                        CurrentDialog.ToggleButtons(true);
                        return;
                    }

                    if (template.MinimumValue.HasValue && petitionValue < template.MinimumValue.Value)
                    {
                        ShowClientErrorMessage(LocalizationUtility.GetLocalizedString("minimumPetitionNumberWarning",
                            new[] { template.MinimumValue.Value }));
                        CurrentDialog.ToggleButtons(true);
                        return;
                    }

                    if (template.MaximumValue.HasValue && petitionValue > template.MaximumValue.Value)
                    {
                        ShowClientErrorMessage(LocalizationUtility.GetLocalizedString("maximumPetitionNumberWarning",
                            new[] { template.MaximumValue.Value }));
                        CurrentDialog.ToggleButtons(true);
                        return;
                    }

                    break;

                default:
                    throw new UnityException($"Unknown value type \"{template.ValueType}\"!");
            }

            string petitionText = DialogUtility.CreatePetitionText(template, petitionValue);
            RegisterNewPetition(petitionText, template.ID, petitionValue);
        }

        private void RegisterNewPetition(string petitionText, int petitionTemplateID, int petitionValue)
        {
            StartCoroutine(BackendConnection.RegisterNewPetition(response =>
            {
                if (HandlePossibleErrorResponse(response))
                {
                    if (response.TryGetResponseAsInt(out int petitionID))
                    {
                        WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnPetitionRegistered,
                            GameEventResult.Success, petitionText);

                        currentPetitionID = petitionID;
                        WorldManager.PlayerManager.StartNewRegisterPetitionVote(this, petitionID);
                    }
                    else
                    {
                        WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnPetitionRegistered,
                            GameEventResult.NotAllowed, petitionText);
                        ShowNotAllowedMessage(
                            LocalizationUtility.GetLocalizedString("registerPetitionNotAllowedMessage"));
                        CurrentDialog.ToggleButtons(true);
                    }
                }
                else
                {
                    WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnPetitionRegistered,
                        GameEventResult.Error, petitionText);
                    CurrentDialog.ToggleButtons(true);
                }
            }, petitionTemplateID, petitionValue, true));
        }

        public void OpenPetition()
        {
            if (!currentPetitionID.HasValue)
            {
                Debug.LogError("Could not register new petition, because values are missing.");
                CancelOpeningPetition();
                return;
            }

            int petitionID = currentPetitionID.Value;

            StartCoroutine(BackendConnection.OpenPetition(response =>
            {
                if (HandlePossibleErrorResponse(response))
                {
                    ReactToGenericBoolResponse(
                        response,
                        () =>
                        {
                            WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnPetitionChanged,
                                GameEventResult.Success, PetitionAction.Open, petitionID);
                            ShowSuccessMessage(LocalizationUtility.GetLocalizedString("openPetitionSuccessMessage"));
                            OpenTemplateList();
                        },
                        () =>
                        {
                            WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnPetitionChanged,
                                GameEventResult.NotAllowed, PetitionAction.Open, petitionID);
                            ShowNotAllowedMessage(
                                LocalizationUtility.GetLocalizedString("openPetitionNotAllowedMessage"));
                            OpenTemplateList();
                        });
                }
                else
                {
                    WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnPetitionChanged,
                        GameEventResult.Error, PetitionAction.Open, petitionID);
                    OpenTemplateList();
                }
            }, petitionID, true));
        }

        public void CancelOpeningPetition()
        {
            currentPetitionID = null;

            OpenTemplateList();
        }

        private void AskToClosePetition(int petitionID, string petitionText, CustomButton closeButton)
        {
            closePetitionDialog = WorldManager.canvasHandler.InitializeYesNoDialog(
                LocalizationUtility.GetLocalizedString("closePetitionTitle"),
                LocalizationUtility.GetLocalizedString("closePetitionMessage") + "\n\n" + petitionText,
                LocalizationUtility.GetLocalizedString("yes"),
                LocalizationUtility.GetLocalizedString("no"), () =>
                {
                    Destroy(closePetitionDialog.gameObject);
                    ClosePetition(petitionID, closeButton);
                    CurrentDialog.gameObject.SetActive(true);
                }, () =>
                {
                    Destroy(closePetitionDialog.gameObject);
                    CurrentDialog.gameObject.SetActive(true);
                }
            );
            CurrentDialog.gameObject.SetActive(false);
        }

        private void ClosePetition(int petitionID, CustomButton closeButton)
        {
            CurrentDialog.ToggleButtons(false, closeButton);
            StartCoroutine(BackendConnection.ClosePetition(
                response =>
                {
                    if (HandlePossibleErrorResponse(response))
                    {
                        if (response.TryGetResponseAsBool(out bool success))
                        {
                            if (success)
                            {
                                WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnPetitionChanged,
                                    GameEventResult.Success, PetitionAction.Close, petitionID);
                                ShowSuccessMessage(
                                    LocalizationUtility.GetLocalizedString("closePetitionSuccessMessage"));
                                OpenPetitionList();
                            }
                            else
                            {
                                WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnPetitionChanged,
                                    GameEventResult.NotAllowed, PetitionAction.Close, petitionID);
                                ShowNotAllowedMessage(
                                    LocalizationUtility.GetLocalizedString("closePetitionNotAllowedMessage"));
                                CurrentDialog.ToggleButtons(true);
                            }
                        }
                        else
                        {
                            switch (response.GetRawResponse())
                            {
                                case "active":
                                    WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnPetitionChanged,
                                        GameEventResult.Success, PetitionAction.Close, petitionID);

                                    WorldManager.PlayerManager.NotifyNewPetitionEffect(petitionID);

                                    ShowSuccessMessage(
                                        LocalizationUtility.GetLocalizedString(
                                            "closePetitionSuccessWithEffectMessage"));
                                    OpenPetitionList();
                                    break;

                                case "inactive":
                                    WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnPetitionChanged,
                                        GameEventResult.Success, PetitionAction.Close, petitionID);
                                    ShowSuccessMessage(
                                        LocalizationUtility.GetLocalizedString(
                                            "closePetitionSuccessWithoutEffectMessage"));
                                    OpenPetitionList();
                                    break;

                                default:
                                    throw new UnityException(
                                        $"What does \"{response.GetRawResponse()}\" mean here? No documentation in backend.");
                            }
                        }
                    }
                    else
                    {
                        WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnPetitionChanged,
                            GameEventResult.Error, PetitionAction.Close, petitionID);
                        CurrentDialog.ToggleButtons(true);
                    }
                },
                petitionID, true));
        }

        private void AskToDeletePetition(int petitionID, string petitionText, CustomButton closeButton)
        {
            deletePetitionDialog = WorldManager.canvasHandler.InitializeYesNoDialog(
                LocalizationUtility.GetLocalizedString("deletePetitionTitle"),
                LocalizationUtility.GetLocalizedString("deletePetitionMessage") + "\n\n" + petitionText,
                LocalizationUtility.GetLocalizedString("yes"),
                LocalizationUtility.GetLocalizedString("no"), () =>
                {
                    Destroy(deletePetitionDialog.gameObject);
                    DeletePetition(petitionID, closeButton);
                    CurrentDialog.gameObject.SetActive(true);
                }, () =>
                {
                    Destroy(deletePetitionDialog.gameObject);
                    CurrentDialog.gameObject.SetActive(true);
                }
            );
            CurrentDialog.gameObject.SetActive(false);
        }

        private void DeletePetition(int petitionID, CustomButton deleteButton)
        {
            CurrentDialog.ToggleButtons(false, deleteButton);
            StartCoroutine(BackendConnection.DeletePetition(
                response =>
                {
                    if (HandlePossibleErrorResponse(response))
                    {
                        ReactToGenericBoolResponse(
                            response,
                            () =>
                            {
                                WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnPetitionChanged,
                                    GameEventResult.Success, PetitionAction.Delete, petitionID);
                                ShowSuccessMessage(
                                    LocalizationUtility.GetLocalizedString("deletePetitionSuccessMessage"));
                                OpenPetitionList();
                            },
                            () =>
                            {
                                WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnPetitionChanged,
                                    GameEventResult.NotAllowed, PetitionAction.Delete, petitionID);
                                ShowNotAllowedMessage(
                                    LocalizationUtility.GetLocalizedString("deletePetitionNotAllowedMessage"));
                                CurrentDialog.ToggleButtons(true);
                            });
                    }
                    else
                    {
                        WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnPetitionChanged,
                            GameEventResult.Error, PetitionAction.Delete, petitionID);
                        CurrentDialog.ToggleButtons(true);
                    }
                },
                petitionID, true));
        }
    }
}