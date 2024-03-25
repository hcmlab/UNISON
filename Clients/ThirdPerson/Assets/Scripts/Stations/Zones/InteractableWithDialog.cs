using UI.Dialogs;

namespace Stations.Zones
{
    public abstract class InteractableWithDialog<TDialog> : Interactable
        where TDialog : SimpleDialog
    {
        protected TDialog CurrentDialog;

        protected void OnCloseDialog()
        {
            CloseInfoMessage();
            OnActionFinished();
        }

        protected void CloseInfoMessage()
        {
            if (CurrentDialog && CurrentDialog.gameObject)
            {
                Destroy(CurrentDialog.gameObject);
                CurrentDialog = null;
            }
        }
    }
}