using TMPro;
using UnityEngine;

namespace UI.Dialogs
{
    public class ToggleAndSpendDialog : ToggleDialog, IToggleButtonsDialog, ISpendDialog
    {
        [SerializeField] private TMP_InputField moneyUnitsInputField;

        public string GetMoneyUnitsText()
        {
            return moneyUnitsInputField.text;
        }

        public void ResetMoneyUnitsText()
        {
            moneyUnitsInputField.text = "0";
        }
    }
}