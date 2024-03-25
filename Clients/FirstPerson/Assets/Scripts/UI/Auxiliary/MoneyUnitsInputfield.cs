using Settings;
using TMPro;
using UnityEngine;

namespace UI.Auxiliary
{
    public class MoneyUnitsInputfield : MonoBehaviour
    {
        [SerializeField] private TMP_Text currencySymbol;

        private void Awake()
        {
            currencySymbol.text = LocalizationUtility.GetLocalizedString("currencySymbol");
        }
    }
}