using UnityEngine;
using World;

namespace Effects
{
    public class Sky : MonoBehaviour
    {
        [SerializeField] private Light skyLight;
        [SerializeField] private Color daylightTransitionColor;
        [SerializeField] private Color daylightColor;
        [SerializeField] private Color eveningTransitionColor;
        [SerializeField] private Color eveningColor;

        [SerializeField] private float daylightIntensity = 1;
        [SerializeField] private float eveningIntensity;
        [SerializeField] private float transitionIntensity = 0.8f;

        [SerializeField] private float daylightXAngle = 60;
        [SerializeField] private float eveningXAngle = 75;
        [SerializeField] private float transitionXAngle = 30;

        [SerializeField] private Material material;
        [SerializeField] private string materialKey = "Vector1_92eaf7d2598c4e86af5d7bb0379c1aaa";
        [SerializeField] private float materialTransitionPoint = 0.25f;

        public bool inInterior;
        private float circledProgress;

        private float? firstEveningTimestamp;
        private WorldManager worldManager;

        private void Awake()
        {
            worldManager = GetComponent<WorldManager>();
        }

        private void Update()
        {
            float daytimeProgress;
            if (worldManager.DayState == DayState.Evening)
            {
                if (firstEveningTimestamp.HasValue)
                {
                    var difference = Time.time - firstEveningTimestamp.Value;
                    daytimeProgress = Mathf.Lerp(0f, 0.5f, Mathf.Min(1f, difference / 60f));
                }
                else
                {
                    firstEveningTimestamp = Time.time;
                    daytimeProgress = 0f;
                }
            }
            else
            {
                firstEveningTimestamp = null;
                daytimeProgress = worldManager.DaytimeProgress;
            }

            circledProgress = Mathf.Sin(Mathf.PI * daytimeProgress);

            UpdateLight();
            UpdateMaterial();
        }

        public void OnDisable()
        {
            material.SetFloat(materialKey, 0);
        }

        public void OnApplicationQuit()
        {
            material.SetFloat(materialKey, 0);
        }

        private void UpdateLight()
        {
            if (!skyLight)
            {
                return;
            }

            if (inInterior)
            {
                skyLight.color = Color.white;
                skyLight.intensity = 1.0f;
                skyLight.transform.rotation = Quaternion.Euler(60, 0, 0);
                return;
            }

            Color color, transitionColor;
            float intensity, x, y;
            switch (worldManager.DayState)
            {
                case DayState.Daylight:
                    transitionColor = worldManager.DaytimeProgress < 0.5f
                        ? daylightTransitionColor
                        : eveningTransitionColor;
                    color = Color.Lerp(transitionColor, daylightColor, circledProgress);
                    intensity = Mathf.Lerp(transitionIntensity, daylightIntensity, circledProgress);
                    x = Mathf.Lerp(transitionXAngle, daylightXAngle, circledProgress);
                    y = Mathf.Lerp(-90, 90, worldManager.DaytimeProgress);
                    break;

                case DayState.Dusk:
                    color = eveningTransitionColor;
                    intensity = transitionIntensity;
                    x = transitionXAngle;
                    y = 90;
                    break;

                case DayState.Evening:
                    transitionColor = worldManager.DaytimeProgress < 0.5f
                        ? eveningTransitionColor
                        : daylightTransitionColor;
                    color = Color.Lerp(transitionColor, eveningColor, circledProgress);
                    intensity = Mathf.Lerp(transitionIntensity, eveningIntensity, circledProgress);
                    x = Mathf.Lerp(transitionXAngle, eveningXAngle, circledProgress);
                    y = Mathf.Lerp(90, 270, worldManager.DaytimeProgress);
                    break;

                default:
                    color = daylightTransitionColor;
                    intensity = transitionIntensity;
                    x = transitionXAngle;
                    y = 0;
                    break;
            }

            skyLight.color = color;
            skyLight.intensity = intensity;
            // Debug.Log($"x: {x}, y: {y}");
            skyLight.transform.rotation = Quaternion.Euler(x, y, 0);
        }

        private void UpdateMaterial()
        {
            if (!material || inInterior)
            {
                return;
            }

            var interpolation = worldManager.DayState switch
            {
                DayState.Daylight => Mathf.Lerp(materialTransitionPoint, 0, circledProgress),
                DayState.Dusk => materialTransitionPoint,
                DayState.Evening => Mathf.Lerp(materialTransitionPoint, 1, circledProgress),
                _ => materialTransitionPoint
            };
            material.SetFloat(materialKey, interpolation);
        }
    }
}