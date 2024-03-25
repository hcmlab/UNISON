using Stations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using World;

namespace UI.HUD
{
    /// <summary>
    /// A single element in [StationsOverviewHUD]
    /// </summary>
    public class StationOverviewObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image stationImage;
        [SerializeField] private Image highlighterImage;
        [SerializeField] private Image jumpToImage;
        private StationType stationType;

        private WorldManager worldManager;

        public void Setup(StationType newStationType, bool hoverEnabled, WorldManager newWorldManager)
        {
            UpdateStation(newStationType);
            worldManager = newWorldManager;
            HoverEnabled = hoverEnabled;
        }

        public void UpdateStation(StationType newStationType)
        {
            stationType = newStationType;
            if (stationType == StationType.None)
            {
                return;
            }

            Sprite s = Resources.Load<Sprite>($"Sprites/StationIcons/{stationType}");
            stationImage.sprite = s;
        }

        public void SetHighlighted(StationType currentStationType) =>
            highlighterImage.gameObject.SetActive(stationType == currentStationType);

        public void SetHighlighted(bool highlighted) => highlighterImage.gameObject.SetActive(highlighted);

        private bool HoverEnabled { get; set; } = true;

        public void OnStationPressed()
        {
            worldManager.MasterManager.FadeToStation(stationType);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (HoverEnabled)
            {
                jumpToImage.gameObject.SetActive(true);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (HoverEnabled)
            {
                jumpToImage.gameObject.SetActive(false);
            }
        }
    }
}