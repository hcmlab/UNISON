using System.Linq;
using TMPro;
using UI.Auxiliary;
using UnityEngine;
using UnityEngine.Events;

namespace UI.Dialogs
{
    public class VoteDialog : PetitionListDialog
    {
        [SerializeField] private GameObject voteOnPetitionContent;
        [SerializeField] private TMP_Text petitionText;
        public CustomButton voteYesButton;
        public CustomButton voteNoButton;

        public override void ToggleButtons(bool value, params CustomButton[] loadingButtons)
        {
            base.ToggleButtons(value, loadingButtons);

            voteYesButton.UpdateButtonState(value
                ? ButtonState.Active
                : loadingButtons.Contains(voteYesButton)
                    ? ButtonState.Loading
                    : ButtonState.Inactive);

            voteNoButton.UpdateButtonState(value
                ? ButtonState.Active
                : loadingButtons.Contains(voteNoButton)
                    ? ButtonState.Loading
                    : ButtonState.Inactive);
        }

        public void UpdateVoteOnPetitionContent(string newPetitionText, UnityAction voteYesButtonListener,
            UnityAction voteNoButtonListener)
        {
            petitionText.text = newPetitionText;

            voteYesButton.OnClick.RemoveAllListeners();
            voteYesButton.OnClick.AddListener(voteYesButtonListener);

            voteNoButton.OnClick.RemoveAllListeners();
            voteNoButton.OnClick.AddListener(voteNoButtonListener);
        }

        public override void OpenPetitionList()
        {
            petitionListContent.SetActive(true);
            voteOnPetitionContent.SetActive(false);
        }

        public void OpenVoteOnPetition()
        {
            petitionListContent.SetActive(false);
            voteOnPetitionContent.SetActive(true);
        }
    }
}