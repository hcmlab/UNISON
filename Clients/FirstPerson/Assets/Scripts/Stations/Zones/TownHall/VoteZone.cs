using System.Collections.Generic;
using Backend;
using Backend.ResponseDataCapsules;
using PUN;
using UI.Auxiliary;
using UI.Dialogs;
using UnityEngine;

namespace Stations.Zones.TownHall
{
    public class VoteZone : InteractableWithDialog<VoteDialog>
    {
        [SerializeField] private VoteItem voteItemPrefab;

        protected override ZoneType ZoneType => ZoneType.Vote;
        protected override string ActionSpriteName => "Vote";

        private int currentPetitionID;
        private bool currentPositiveVote;

        private bool isInitialOpenPetitionList;

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
                CurrentDialog.ToggleButtons(false, CurrentDialog.backToPetitionListButton);
            }

            StartCoroutine(BackendConnection.GetOpenPetitions(HandleOpenPetitionList));
        }

        private void HandleOpenPetitionList(ParameterizedResponse<List<Petition>> response)
        {
            if (HandlePossibleErrorResponse(response))
            {
                if (isInitialOpenPetitionList)
                {
                    CurrentDialog = WorldManager.canvasHandler.InitializeVoteDialog(
                        LocalizationUtility.GetLocalizedString("voteTitle"),
                        LocalizationUtility.GetLocalizedString("voteMessage"),
                        LocalizationUtility.GetLocalizedString("close"),
                        null,
                        OnCloseDialog,
                        null
                    );

                    isInitialOpenPetitionList = false;
                }

                CurrentDialog.OpenPetitionList();
                AddDialogComponents(response.Data);
            }

            CurrentDialog.ToggleButtons(true);
        }

        private void AddDialogComponents(List<Petition> openPetitions)
        {
            // Clear all previous petitions.
            foreach (Transform child in CurrentDialog.GetPetitionsTransform())
            {
                Destroy(child.gameObject);
            }

            foreach (Petition openPetition in openPetitions)
            {
                VoteItem voteItem = Instantiate(voteItemPrefab, CurrentDialog.GetPetitionsTransform());

                string voteText = DialogUtility.CreatePetitionText(openPetition);
                voteItem.summary.text = voteText;

                voteItem.button.OnClick.AddListener(() => OpenVoteOnPetition(openPetition.ID, voteText));
            }
        }

        private void OpenVoteOnPetition(int petitionID, string petitionText)
        {
            CurrentDialog.OpenVoteOnPetition();

            CurrentDialog.UpdateVoteOnPetitionContent(
                petitionText,
                () => VoteOnPetition(petitionID, true),
                () => VoteOnPetition(petitionID, false)
            );
            CurrentDialog.AddBackToPetitionListClickListener(OpenPetitionList);
        }

        private void VoteOnPetition(int petitionID, bool positiveVote)
        {
            CurrentDialog.ToggleButtons(false, CurrentDialog.voteYesButton, CurrentDialog.voteNoButton);

            currentPetitionID = petitionID;
            currentPositiveVote = positiveVote;

            StartCoroutine(BackendConnection.Vote(HandleVoteOnPetition, petitionID, positiveVote));
        }

        private void HandleVoteOnPetition(Response response)
        {
            if (HandlePossibleErrorResponse(response))
            {
                WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnVoted, GameEventResult.Success,
                    currentPetitionID, currentPositiveVote);
                ShowSuccessMessage(LocalizationUtility.GetLocalizedString("voteSuccessMessage"), OpenPetitionList);
            }
            else
            {
                WorldManager.ClientManager.PunClient.SendNewEvent(GameEventType.OnVoted, GameEventResult.Error,
                    currentPetitionID, currentPositiveVote);
            }
        }
    }
}