using UI.Auxiliary;

namespace UI.Dialogs
{
    public interface IToggleButtonsDialog
    {
        public void ToggleButtons(bool value, params CustomButton[] loadingButtons);
    }
}