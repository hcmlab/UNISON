using UnityEngine;

namespace Settings
{
    /// <summary>
    ///     This class holds the control settings of the game.
    /// </summary>
    public class ControlSettings : MonoBehaviour
    {
        public static ControlData ControlData;
        [SerializeField] private DefaultControlSettings defaultControlSettings;

        private void Awake()
        {
            LoadControlData();
        }

        private void LoadControlData()
        {
            ControlData = LoadSave.LoadControlDataFromFile() ?? new ControlData(defaultControlSettings);
            ControlData?.ReplaceNoneWithDefault(defaultControlSettings);
        }
    }
}