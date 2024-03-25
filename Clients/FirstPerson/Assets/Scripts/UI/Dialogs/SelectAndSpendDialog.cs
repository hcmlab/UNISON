using TMPro;
using UnityEngine;

namespace UI.Dialogs
{
    public class SelectAndSpendDialog : SelectDialog, ISpendDialog
    {
        [SerializeField] private TMP_InputField moneyUnitsInputField;

        public string GetMoneyUnitsText() => moneyUnitsInputField.text;

        public void ResetMoneyUnitsText() => moneyUnitsInputField.text = "0";
    }
}