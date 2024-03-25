using System.Linq;
using TMPro;
using UI.Auxiliary;
using UnityEngine;
using UnityEngine.Events;

namespace UI.Dialogs
{
    public class PetitionDialog : PetitionListDialog
    {
        [SerializeField] private GameObject templateListContent;
        [SerializeField] private GameObject templates;

        [SerializeField] private GameObject petitionTemplateContent;
        [SerializeField] private GameObject intPetitionTemplate;
        [SerializeField] private GameObject boolPetitionTemplate;
        [SerializeField] private TMP_Text petitionSummary;
        [SerializeField] private TMP_InputField petitionValue;
        public CustomButton openPetitionButton;
        public CustomButton backToTemplateListButton;

        public override void ToggleButtons(bool value, params CustomButton[] loadingButtons)
        {
            base.ToggleButtons(value, loadingButtons);

            openPetitionButton.UpdateButtonState(value
                ? ButtonState.Active
                : loadingButtons.Contains(openPetitionButton)
                    ? ButtonState.Loading
                    : ButtonState.Inactive);

            backToTemplateListButton.UpdateButtonState(value
                ? ButtonState.Active
                : loadingButtons.Contains(backToTemplateListButton)
                    ? ButtonState.Loading
                    : ButtonState.Inactive);

            if (templates.activeSelf)
            {
                foreach (var customButton in templates.GetComponentsInChildren<CustomButton>())
                {
                    customButton.UpdateButtonState(value
                        ? ButtonState.Active
                        : loadingButtons.Contains(customButton)
                            ? ButtonState.Loading
                            : ButtonState.Inactive);
                }
            }
        }

        public override void OpenPetitionList()
        {
            petitionListContent.SetActive(true);
            templateListContent.SetActive(false);
            petitionTemplateContent.SetActive(false);
        }

        public void UpdatePetitionTemplateContent(string petitionSummaryText, UnityAction openPetitionButtonListener,
            UnityAction backToTemplateListButtonListener)
        {
            petitionSummary.text = petitionSummaryText;
            petitionValue.text = "";

            openPetitionButton.OnClick.RemoveAllListeners();
            openPetitionButton.OnClick.AddListener(openPetitionButtonListener);

            backToTemplateListButton.OnClick.RemoveAllListeners();
            backToTemplateListButton.OnClick.AddListener(backToTemplateListButtonListener);
        }

        public string GetPetitionValueText()
        {
            return petitionValue.text;
        }

        public void UpdatePetitionSummaryText(string petitionSummaryText)
        {
            petitionSummary.text = petitionSummaryText;
        }

        public string GetPetitionSummaryText()
        {
            return petitionSummary.text;
        }

        public Transform GetTemplatesTransform()
        {
            return templates.transform;
        }

        public void OpenTemplateList()
        {
            petitionListContent.SetActive(false);
            templateListContent.SetActive(true);
            petitionTemplateContent.SetActive(false);
        }

        public void OpenPetitionTemplate(string valueType)
        {
            petitionListContent.SetActive(false);
            templateListContent.SetActive(false);
            petitionTemplateContent.SetActive(true);

            switch (valueType)
            {
                case "bool":
                    boolPetitionTemplate.SetActive(true);
                    intPetitionTemplate.SetActive(false);
                    break;

                case "int":
                    boolPetitionTemplate.SetActive(false);
                    intPetitionTemplate.SetActive(true);
                    break;

                default:
                    throw new UnityException($"Unknown value type \"{valueType}\"!");
            }
        }
    }
}