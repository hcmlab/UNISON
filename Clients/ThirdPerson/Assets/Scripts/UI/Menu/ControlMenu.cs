using System;
using Backend;
using Settings;
using UI.Auxiliary;
using UI.Dialogs;
using UI.HUD;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Menu
{
    public class ControlMenu : MonoBehaviour
    {
        [SerializeField] private CanvasHandler canvasHandler;
        [SerializeField] private Transform controlScrollView;
        [SerializeField] private KeyBindingButton keyBindingButtonPrefab;
        [SerializeField] private DoubleKeyBindingButton doubleKeyBindingButtonPrefab;
        [SerializeField] private GameObject scrollViewSeparator;
        [SerializeField] private Button resetToDefaultButton;
        [SerializeField] private DefaultControlSettings defaultControlSettings;

        private void Awake()
        {
            resetToDefaultButton.onClick.AddListener(OnResetToDefault);
        }

        private void OnEnable()
        {
            AddAllControlSettings();
        }

        private void OnDisable()
        {
            KeyBindingsOverviewHUD.KeyBindingsChanged = true;

            LoadSave.SaveControlDataToFile(ControlSettings.ControlData);
        }

        private void OnResetToDefault()
        {
            ControlSettings.ControlData = new ControlData(defaultControlSettings);
            AddAllControlSettings();
        }

        private bool CheckDoubleAssignment(KeyCode key)
        {
            foreach (var propertyInfo in ControlSettings.ControlData.GetType().GetProperties())
            {
                var value = propertyInfo.GetValue(ControlSettings.ControlData);
                if ((KeyCode)value == key)
                {
                    canvasHandler.InitializeOkDialog(LocalizationUtility.GetLocalizedString("warningTitle"),
                        LocalizationUtility.GetLocalizedString("keyAlreadyBoundWarning",
                            new[] { LocalizationUtility.GetTextForKeyCode(key), propertyInfo.Name }),
                        LocalizationUtility.GetLocalizedString("ok"), FeedbackType.Negative, null);
                    return false;
                }
            }

            return true;
        }

        private void AddAllControlSettings()
        {
            // Remove all items from scroll view.
            foreach (Transform keyBindingButton in controlScrollView)
            {
                Destroy(keyBindingButton.gameObject);
            }

            // Add player control settings.
            InstantiateDoubleKeyBindingButton(LocalizationUtility.GetLocalizedString("moveForward"),
                ControlSettings.ControlData.PrimaryForward,
                code => { ControlSettings.ControlData.PrimaryForward = code; },
                ControlSettings.ControlData.SecondaryForward,
                code => { ControlSettings.ControlData.SecondaryForward = code; }
            );
            Instantiate(scrollViewSeparator, controlScrollView);

            InstantiateDoubleKeyBindingButton(LocalizationUtility.GetLocalizedString("moveLeft"),
                ControlSettings.ControlData.PrimaryLeft,
                code => { ControlSettings.ControlData.PrimaryLeft = code; },
                ControlSettings.ControlData.SecondaryLeft,
                code => { ControlSettings.ControlData.SecondaryLeft = code; }
            );
            Instantiate(scrollViewSeparator, controlScrollView);

            InstantiateDoubleKeyBindingButton(LocalizationUtility.GetLocalizedString("moveRight"),
                ControlSettings.ControlData.PrimaryRight,
                code => { ControlSettings.ControlData.PrimaryRight = code; },
                ControlSettings.ControlData.SecondaryRight,
                code => { ControlSettings.ControlData.SecondaryRight = code; }
            );
            Instantiate(scrollViewSeparator, controlScrollView);

            InstantiateDoubleKeyBindingButton(LocalizationUtility.GetLocalizedString("moveBackward"),
                ControlSettings.ControlData.PrimaryBackward,
                code => { ControlSettings.ControlData.PrimaryBackward = code; },
                ControlSettings.ControlData.SecondaryBackward,
                code => { ControlSettings.ControlData.SecondaryBackward = code; }
            );
            Instantiate(scrollViewSeparator, controlScrollView);

            InstantiateKeyBindingButton(LocalizationUtility.GetLocalizedString("interact"),
                ControlSettings.ControlData.Interact,
                code => { ControlSettings.ControlData.Interact = code; }
            );
            Instantiate(scrollViewSeparator, controlScrollView);

            InstantiateKeyBindingButton(LocalizationUtility.GetLocalizedString("toggleVoice"),
                ControlSettings.ControlData.ToggleVoice,
                code => { ControlSettings.ControlData.ToggleVoice = code; }
            );
            Instantiate(scrollViewSeparator, controlScrollView);

            InstantiateKeyBindingButton(LocalizationUtility.GetLocalizedString("openMap"),
                ControlSettings.ControlData.OpenMap,
                code => { ControlSettings.ControlData.OpenMap = code; }
            );
            Instantiate(scrollViewSeparator, controlScrollView);

            InstantiateKeyBindingButton(LocalizationUtility.GetLocalizedString("confirmDialog"),
                ControlSettings.ControlData.ConfirmDialog,
                code => { ControlSettings.ControlData.ConfirmDialog = code; }
            );
            Instantiate(scrollViewSeparator, controlScrollView);

            InstantiateKeyBindingButton(LocalizationUtility.GetLocalizedString("cancelDialog"),
                ControlSettings.ControlData.CancelDialog,
                code => { ControlSettings.ControlData.CancelDialog = code; }
            );
            Instantiate(scrollViewSeparator, controlScrollView);

            InstantiateKeyBindingButton(LocalizationUtility.GetLocalizedString("callForHelp"),
                ControlSettings.ControlData.CallForHelp,
                code => { ControlSettings.ControlData.CallForHelp = code; }
            );
            Instantiate(scrollViewSeparator, controlScrollView);

            InstantiateKeyBindingButton(LocalizationUtility.GetLocalizedString("toggleMenu"),
                ControlSettings.ControlData.ToggleMenu,
                code => { ControlSettings.ControlData.ToggleMenu = code; }
            );
            Instantiate(scrollViewSeparator, controlScrollView);

            if (BackendConnection.IsLoggedInAsMasterClient())
            {
                // Add master control settings.
                InstantiateKeyBindingButton(LocalizationUtility.GetLocalizedString("toggleGlobalVoice"),
                    ControlSettings.ControlData.PushToTalkGlobal,
                    code => { ControlSettings.ControlData.PushToTalkGlobal = code; }
                );

                InstantiateKeyBindingButton(LocalizationUtility.GetLocalizedString("toggleStats"),
                    ControlSettings.ControlData.ToggleStats,
                    code => { ControlSettings.ControlData.ToggleStats = code; }
                );
                Instantiate(scrollViewSeparator, controlScrollView);

                InstantiateKeyBindingButton(LocalizationUtility.GetLocalizedString("sprint"),
                    ControlSettings.ControlData.Sprint,
                    code => { ControlSettings.ControlData.Sprint = code; }
                );
                Instantiate(scrollViewSeparator, controlScrollView);
            }
        }

        private void InstantiateKeyBindingButton(string newKeyCodeText, KeyCode initialPrimaryKeyCode,
            Action<KeyCode> updatePrimaryKeyCode)
        {
            var keyBindingButton = Instantiate(keyBindingButtonPrefab, controlScrollView);
            keyBindingButton.SetupKeyBindingButton(newKeyCodeText, initialPrimaryKeyCode, code =>
            {
                if (CheckDoubleAssignment(code))
                {
                    updatePrimaryKeyCode(code);
                    keyBindingButton.UpdatePrimaryKeyCode(code);
                }
            });
        }

        private void InstantiateDoubleKeyBindingButton(string newKeyCodeText, KeyCode initialPrimaryKeyCode,
            Action<KeyCode> updatePrimaryKeyCode, KeyCode initialSecondaryKeyCode,
            Action<KeyCode> updateSecondaryKeyCode)
        {
            var doubleKeyBindingButton =
                Instantiate(doubleKeyBindingButtonPrefab, controlScrollView);
            doubleKeyBindingButton.SetupKeyBindingButton(newKeyCodeText,
                initialPrimaryKeyCode, code =>
                {
                    if (CheckDoubleAssignment(code))
                    {
                        updatePrimaryKeyCode(code);
                        doubleKeyBindingButton.UpdatePrimaryKeyCode(code);
                    }
                }, initialSecondaryKeyCode, code =>
                {
                    if (CheckDoubleAssignment(code))
                    {
                        updateSecondaryKeyCode(code);
                        doubleKeyBindingButton.UpdateSecondaryKeyCode(code);
                    }
                });
        }
    }
}