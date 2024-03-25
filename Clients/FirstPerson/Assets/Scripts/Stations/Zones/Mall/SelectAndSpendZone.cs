using System.Collections.Generic;
using UI.Dialogs;

namespace Stations.Zones.Mall
{
    public abstract class SelectAndSpendZone<TDialog> : SpendZone<TDialog>
        where TDialog : SimpleDialog, IToggleButtonsDialog, ISpendDialog, ISelectDialog
    {
        protected bool TryGetSelected(List<string> values, out string selected)
        {
            int selectedIndex = CurrentDialog.GetIndexOfSelection();

            if (selectedIndex >= 0 && selectedIndex < values.Count)
            {
                selected = values[selectedIndex];
                return true;
            }

            CurrentDialog.ToggleButtons(false);
            ShowClientErrorMessage(LocalizationUtility.GetLocalizedString("selectOptionWarning"),
                () => CurrentDialog.ToggleButtons(true));

            selected = default;
            return false;
        }
    }
}