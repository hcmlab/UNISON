using Stations;
using UI.Menu;
using UnityEngine;
using World;

namespace UI.HUD
{
    /// <summary>
    ///     A small 3rd person camera directly above the player. So he can see his surrounding space
    /// </summary>
    public class MiniMapHUD : MonoBehaviourHUD, IStateSubscriber<Station>
    {
        private const float BigPointerSizeOutside = 16f;
        private const float BigPointerSizeInterior = 24f;
        private const float BigCameraSizeOutside = 48f;
        private const float BigCameraSizeInterior = 12f;

        private const float SmallPointerSizeOutside = 8f;
        private const float SmallPointerSizeInterior = 12f;
        private const float SmallCameraSizeOutside = 24f;
        private const float SmallCameraSizeInterior = 6f;
        [SerializeField] private CanvasGroup canvasGroup;

        [SerializeField] private GameObject bigMapContent;
        [SerializeField] private RectTransform bigPointerRectTransform;

        [SerializeField] private GameObject smallMapContent;
        [SerializeField] private RectTransform smallPointerRectTransform;

        private bool isInterior;

        private void Update()
        {
            if (SettingsMenu.MinimapEnabled)
            {
                if (Input.GetKey(KeyCode.M))
                {
                    bigMapContent.SetActive(true);
                    smallMapContent.SetActive(false);

                    if (isInterior)
                    {
                        ZoomCamSizeTo(BigCameraSizeInterior);
                        SetPointerSize(BigPointerSizeInterior);
                    }
                    else
                    {
                        ZoomCamSizeTo(BigCameraSizeOutside);
                        SetPointerSize(BigPointerSizeOutside);
                    }
                }
                else
                {
                    bigMapContent.SetActive(false);
                    smallMapContent.SetActive(true);

                    if (isInterior)
                    {
                        ZoomCamSizeTo(SmallCameraSizeInterior);
                        SetPointerSize(SmallPointerSizeInterior);
                    }
                    else
                    {
                        ZoomCamSizeTo(SmallCameraSizeOutside);
                        SetPointerSize(SmallPointerSizeOutside);
                    }
                }

                canvasGroup.alpha = 1;
                canvasGroup.interactable = true;
            }
            else
            {
                canvasGroup.alpha = 0;
                canvasGroup.interactable = false;
            }
        }

        public void OnUpdated(Station station, Station oldStation)
        {
            isInterior = station.IsInterior;
        }

        public override void OnBeingEnabled(WorldManager worldManager)
        {
            base.OnBeingEnabled(worldManager);

            CurrentWorldManager.ClientManager.SubscribeToStation(this);
        }

        public override void OnBeingDisabled()
        {
            CurrentWorldManager.ClientManager.UnsubscribeFromStation(this);
        }

        private void SetPointerSize(float pointerSize)
        {
            smallPointerRectTransform.sizeDelta = new Vector2(pointerSize, pointerSize);
            bigPointerRectTransform.sizeDelta = new Vector2(pointerSize, pointerSize);
        }

        private void ZoomCamSizeTo(float value)
        {
            CurrentWorldManager.minimapCamera.orthographicSize = value;
        }
    }
}