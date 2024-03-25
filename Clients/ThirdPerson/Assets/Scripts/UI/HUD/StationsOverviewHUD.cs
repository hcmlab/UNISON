using System.Collections;
using System.Collections.Generic;
using Stations;
using UnityEngine;
using UnityEngine.EventSystems;
using World;

namespace UI.HUD
{
    /// <summary>
    ///     A list of stations for the master. With this, the master client can see his current station and jump to other
    ///     stations.
    /// </summary>
    public class StationsOverviewHUD : MonoBehaviourHUD, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] public GameObject stateShrinkObject;
        [SerializeField] private GameObject stateExpandObject;
        [SerializeField] private Transform horizontalLayoutGroupTransform;
        [SerializeField] private GameObject stationOverviewObjectPrefab;
        [SerializeField] private StationOverviewObject stateShrinkOverviewObject;

        [SerializeField] private float hoverExitDelay;

        private readonly List<StationOverviewObject> instantiatedObjects = new List<StationOverviewObject>();

        private StationType currentStationType;

        private bool expandActive;
        private bool isHovering;
        private float timeStampToShrink;

        private void Update()
        {
            // TODO: Optimize with subscriber

            var currentStation = CurrentWorldManager.MasterManager.PunClient.CurrentStation;
            if (!currentStation)
            {
                return;
            }

            currentStationType = currentStation.Type;
            if (currentStationType == StationType.None)
            {
                return;
            }

            SetCurrentStationImage();
            if (isHovering)
            {
                timeStampToShrink = Time.time + hoverExitDelay;
            }

            ToggleStates();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovering = true;
            expandActive = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovering = false;
            StartCoroutine(HideListDelayed());
        }

        public override void OnBeingEnabled(WorldManager worldManager)
        {
            base.OnBeingEnabled(worldManager);

            stateShrinkOverviewObject.SetHighlighted(false);
            stateShrinkOverviewObject.Setup(currentStationType, false, null);
            InitStationsInExpanded();
        }

        private void InitStationsInExpanded()
        {
            foreach (var s in CurrentWorldManager.stations)
            {
                if (s.Type == StationType.None)
                {
                    continue;
                }

                var g = Instantiate(stationOverviewObjectPrefab, horizontalLayoutGroupTransform);
                g.transform.SetSiblingIndex(horizontalLayoutGroupTransform.childCount - 2);
                var soo = g.GetComponent<StationOverviewObject>();
                soo.Setup(s.Type, true, CurrentWorldManager);
                // soo.UpdateStation(s);
                instantiatedObjects.Add(soo);
            }
        }

        private void SetCurrentStationImage()
        {
            stateShrinkOverviewObject.UpdateStation(currentStationType);
        }

        private IEnumerator HideListDelayed()
        {
            yield return new WaitForSeconds(hoverExitDelay);
            if (Time.time > timeStampToShrink)
            {
                expandActive = false;
            }

            yield return null;
        }

        private void ToggleStates()
        {
            if (expandActive)
            {
                foreach (var soo in instantiatedObjects)
                {
                    soo.SetHighlighted(currentStationType);
                }
            }

            stateShrinkObject.SetActive(!expandActive);
            stateExpandObject.SetActive(expandActive);
        }
    }
}