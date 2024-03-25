using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UI.Auxiliary
{
    /// <summary>
    /// Add this Script to member of the canvas to get a callback
    /// This does not work like "OnMouseDown" => This is for objects with colliders or objects, that are triggers
    /// </summary>
    public class OnClickedCallback : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private UnityEvent onClick;

        public void OnPointerDown(PointerEventData eventData)
        {
            onClick.Invoke();
        }
    }
}