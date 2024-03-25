using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Menu
{
    public class SettingsMenu : MonoBehaviour
    {
        [SerializeField] private Button syringeButton;
        [SerializeField] private Button hospitalSoundButton;
        [SerializeField] private Button keyBindingsOverviewButton;
        [SerializeField] private Button minimapButton;
        [SerializeField] private TMP_Dropdown fullScreenModeDropdown;

        public static bool SyringesEnabled { get; private set; } = true;
        public static bool HospitalSoundEnabled { get; private set; } = true;
        public static bool KeyBindingsOverviewEnabled { get; private set; } = true;
        public static bool MinimapEnabled { get; private set; } = true;
        
        private List<FullScreenMode> fullScreenModes;

        private void Awake()
        {
            syringeButton.onClick.AddListener(OnSyringeButton);
            hospitalSoundButton.onClick.AddListener(OnHospitalSoundButton);
            keyBindingsOverviewButton.onClick.AddListener(OnKeyBindingsOverviewButton);
            minimapButton.onClick.AddListener(OnMinimapButton);

            fullScreenModes = Enum.GetValues(typeof(FullScreenMode)).Cast<FullScreenMode>().ToList();
        }

        private void Start()
        {
            SwitchButton(syringeButton, SyringesEnabled);
            SwitchButton(hospitalSoundButton, HospitalSoundEnabled);
            SwitchButton(keyBindingsOverviewButton, KeyBindingsOverviewEnabled);
            SwitchButton(minimapButton, MinimapEnabled);
        }

        private void OnEnable()
        {
            fullScreenModeDropdown.ClearOptions();
            List<string> fullScreenModeOptions = new List<string>();
            foreach (FullScreenMode fullScreenMode in fullScreenModes)
            {
                switch (fullScreenMode)
                {
                    case FullScreenMode.Windowed:
                        fullScreenModeOptions.Add(LocalizationUtility.GetLocalizedString("windowed"));
                        break;
                    case FullScreenMode.MaximizedWindow:
                        fullScreenModeOptions.Add(LocalizationUtility.GetLocalizedString("maximizedWindowed"));
                        break;
                    case FullScreenMode.ExclusiveFullScreen:
                        fullScreenModeOptions.Add(LocalizationUtility.GetLocalizedString("exclusiveFullScreen"));
                        break;
                    case FullScreenMode.FullScreenWindow:
                        fullScreenModeOptions.Add(LocalizationUtility.GetLocalizedString("fullScreenWindow"));
                        break;
                    default:
                        throw new UnityException($"Unknown full screen mode {fullScreenMode}!");
                }
            }

            fullScreenModeDropdown.AddOptions(fullScreenModeOptions);
            fullScreenModeDropdown.value = fullScreenModes.FindIndex(x => x == Screen.fullScreenMode);
            fullScreenModeDropdown.onValueChanged.RemoveAllListeners();
            fullScreenModeDropdown.onValueChanged.AddListener(OnFullScreenMode);
        }

        private static void SwitchButton(Button button, bool on)
        {
            button.gameObject.GetComponentInChildren<TMP_Text>().text = @on ? "On" : "Off";
            button.gameObject.GetComponent<Image>().color =
                @on ? new Color32(84, 196, 0, 255) : new Color32(240, 103, 57, 255);
        }

        private void OnSyringeButton()
        {
            SyringesEnabled = !SyringesEnabled;
            SwitchButton(syringeButton, SyringesEnabled);
        }

        private void OnHospitalSoundButton()
        {
            HospitalSoundEnabled = !HospitalSoundEnabled;
            SwitchButton(hospitalSoundButton, HospitalSoundEnabled);
        }

        private void OnKeyBindingsOverviewButton()
        {
            KeyBindingsOverviewEnabled =
                !KeyBindingsOverviewEnabled;
            SwitchButton(keyBindingsOverviewButton, KeyBindingsOverviewEnabled);
        }

        private void OnMinimapButton()
        {
            MinimapEnabled = !MinimapEnabled;
            SwitchButton(minimapButton, MinimapEnabled);
        }

        private void OnFullScreenMode(int index)
        {
            Screen.fullScreenMode = fullScreenModes[index];
        }
    }
}