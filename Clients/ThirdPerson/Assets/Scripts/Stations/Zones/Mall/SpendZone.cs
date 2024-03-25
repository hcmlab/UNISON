using System.Globalization;
using UI.Dialogs;

namespace Stations.Zones.Mall
{
    public abstract class SpendZone<TDialog> : InteractableWithDialog<TDialog>
        where TDialog : SimpleDialog, IToggleButtonsDialog, ISpendDialog
    {
        protected bool TryGetMoneyUnits(out double moneyUnits)
        {
            if (string.IsNullOrWhiteSpace(CurrentDialog.GetMoneyUnitsText()))
            {
                CurrentDialog.ToggleButtons(false);
                ShowClientErrorMessage(LocalizationUtility.GetLocalizedString("numberMissingWarning"),
                    () => CurrentDialog.ToggleButtons(true));
            }
            else if (double.TryParse(CurrentDialog.GetMoneyUnitsText(), NumberStyles.Float,
                         CultureInfo.InvariantCulture,
                         out var result))
            {
                if (result > 0)
                {
                    moneyUnits = result;
                    return true;
                }

                CurrentDialog.ToggleButtons(false);
                ShowClientErrorMessage(LocalizationUtility.GetLocalizedString("numberPositiveWarning"),
                    () => CurrentDialog.ToggleButtons(true));
            }
            else
            {
                CurrentDialog.ToggleButtons(false);
                ShowClientErrorMessage(LocalizationUtility.GetLocalizedString("numberValidWarning"),
                    () => CurrentDialog.ToggleButtons(true));
            }

            moneyUnits = default;
            return false;
        }
    }
}