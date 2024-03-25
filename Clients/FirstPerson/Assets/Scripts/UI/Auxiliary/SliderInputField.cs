using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.Auxiliary
{
    public class SliderInputField : MonoBehaviour
    {
        [SerializeField] private TMP_Text minText;
        [SerializeField] private TMP_Text maxText;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Slider slider;

        private float currentValue;
        private float stepSize;
        private bool flexibleMax;

        private UnityAction<float> onSliderChanged;

        public float Value => currentValue * stepSize;

        private void Awake()
        {
            inputField.onEndEdit.AddListener(InputFieldChanged);
            slider.onValueChanged.AddListener(OnSliderChanged);
        }

        public void SetupSlider(float min, float max, float standard, UnityAction<float> newOnSliderChanged = null,
            float sliderStepSize = 1, bool flexibleSliderMax = false)
        {
            stepSize = sliderStepSize;
            flexibleMax = flexibleSliderMax;

            slider.minValue = min / sliderStepSize;
            slider.maxValue = max / sliderStepSize;
            minText.text = min.ToString("N0", CultureInfo.InvariantCulture);
            maxText.text = max.ToString("N0", CultureInfo.InvariantCulture);
            slider.value = standard / sliderStepSize;
            inputField.text = standard.ToString("N0", CultureInfo.InvariantCulture);
            currentValue = standard / sliderStepSize;

            onSliderChanged = newOnSliderChanged;
        }

        public void SetMaximum(float max)
        {
            slider.maxValue = max / stepSize;
            maxText.text = max.ToString("N0", CultureInfo.InvariantCulture);
        }

        private void UpdateMaximum(float max)
        {
            slider.maxValue = max;
            maxText.text = (max * stepSize).ToString("N0", CultureInfo.InvariantCulture);
        }

        private void InputFieldChanged(string val)
        {
            float newValue;
            try
            {
                newValue = float.Parse(val) / stepSize;
            }
            catch (FormatException)
            {
                newValue = 0;
            }

            if (newValue > slider.maxValue)
            {
                if (flexibleMax)
                {
                    UpdateMaximum(newValue);
                    slider.value = newValue;
                }
                else
                {
                    slider.value = slider.maxValue;
                }
            }
            else if (newValue < slider.minValue)
            {
                slider.value = slider.minValue;
            }
            else
            {
                slider.value = newValue;
            }

            currentValue = slider.value;
        }

        private void OnSliderChanged(float value)
        {
            float newValue = value * stepSize;
            inputField.text = newValue.ToString("N0", CultureInfo.InvariantCulture);
            currentValue = value;

            onSliderChanged?.Invoke(newValue);
        }
    }
}