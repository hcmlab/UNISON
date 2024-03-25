using UI.Auxiliary;
using UnityEngine;
using UnityEngine.Events;

namespace UI.Dialogs
{
    public class HelpDeskDialog : OkDialog
    {
        [SerializeField] private CustomButton[] generalButtons;
        [SerializeField] private GameObject[] generalContents;

        [SerializeField] private CustomButton[] overviewButtons;
        [SerializeField] private GameObject[] overviewContents;

        [SerializeField] private CustomButton[] stationsButtons;
        [SerializeField] private GameObject[] stationsContents;

        [SerializeField] private CustomButton[] attributesButtons;
        [SerializeField] private GameObject[] attributesContents;

        [SerializeField] private CustomButton[] settingsButtons;
        [SerializeField] private GameObject[] settingsContents;

        public void Setup(UnityAction confirmAction = null)
        {
            base.Setup(null, null, LocalizationUtility.GetLocalizedString("close"), confirmAction);

            AddOnClickListeners(settingsButtons, settingsContents);
            AddOnClickListeners(attributesButtons, attributesContents);
            AddOnClickListeners(stationsButtons, stationsContents);
            AddOnClickListeners(overviewButtons, overviewContents);
            AddOnClickListeners(generalButtons, generalContents);
        }

        private static void AddOnClickListeners(CustomButton[] buttons, GameObject[] contents)
        {
            for (var outerIndex = 0; outerIndex < buttons.Length; ++outerIndex)
            {
                var currentIndex = outerIndex;

                buttons[currentIndex].OnClick.AddListener(() =>
                {
                    for (var innerIndex = 0; innerIndex < buttons.Length; ++innerIndex)
                    {
                        buttons[innerIndex].Interactable = currentIndex != innerIndex;
                        contents[innerIndex].SetActive(currentIndex == innerIndex);
                    }
                });
            }

            buttons[0].OnClick.Invoke();
        }
    }
}