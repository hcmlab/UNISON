using System.Linq;
using UI.Auxiliary;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.Dialogs
{
    public abstract class PetitionListDialog : YesNoDialog
    {
        [SerializeField] protected GameObject petitionListContent;
        [SerializeField] private GameObject petitions;
        public CustomButton backToPetitionListButton;

        public abstract void OpenPetitionList();

        public override void ToggleButtons(bool value, params CustomButton[] loadingButtons)
        {
            base.ToggleButtons(value, loadingButtons);

            backToPetitionListButton.UpdateButtonState(value
                ? ButtonState.Active
                : loadingButtons.Contains(backToPetitionListButton)
                    ? ButtonState.Loading
                    : ButtonState.Inactive);

            if (petitions.activeSelf)
            {
                foreach (CustomButton customButton in petitions.GetComponentsInChildren<CustomButton>())
                {
                    customButton.UpdateButtonState(value
                        ? ButtonState.Active
                        : loadingButtons.Contains(customButton)
                            ? ButtonState.Loading
                            : ButtonState.Inactive);
                }
            }
        }

        public Transform GetPetitionsTransform() => petitions.transform;

        public void AddBackToPetitionListClickListener(UnityAction call)
        {
            backToPetitionListButton.OnClick.RemoveAllListeners();
            backToPetitionListButton.OnClick.AddListener(call);
        }
    }
}