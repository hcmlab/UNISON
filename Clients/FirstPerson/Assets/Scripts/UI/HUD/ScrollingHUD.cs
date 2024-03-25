using UnityEngine;
using UnityEngine.EventSystems;
using World;

namespace UI.HUD
{
    /// <summary>
    /// HUD scripts, that have a ScrollView inside, can extend this class. It notifies the ClientManager, that the mouse is scrolling in a ScrollView and prevents unwanted parallel scrolling.
    /// </summary>
    public abstract class ScrollingHUD : MonoBehaviourHUD, IPointerEnterHandler, IPointerExitHandler
    {
        protected bool PointerOnView { get; private set; }

        public void OnPointerEnter(PointerEventData eventData)
        {
            PointerOnView = true;
            CurrentWorldManager.ClientManager.IsClientMouseOnScrollingHUD = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            PointerOnView = false;
            CurrentWorldManager.ClientManager.IsClientMouseOnScrollingHUD = false;
        }
    }
}