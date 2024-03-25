using UnityEngine;

namespace Settings
{
    /// <summary>
    /// This class holds the control settings of the game.
    /// </summary>
    public class ControlSettings : MonoBehaviour
    {
        [SerializeField] private DefaultControlSettings defaultControlSettings;
        
        public static ControlData ControlData;

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

