using System;
using System.Reflection;
using UnityEngine;

namespace Settings
{
    [Serializable]
    public class ControlData
    {
        public ControlData()
        {
        } // Necessary for deserialization

        public ControlData(DefaultControlSettings defaultControlSettings)
        {
            var propertyInfos =
                typeof(ControlData).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var propertyInfo in propertyInfos)
            {
                propertyInfo.SetValue(this,
                    defaultControlSettings.GetType().GetProperty(propertyInfo.Name,
                            BindingFlags.Instance | BindingFlags.Public)!
                        .GetValue(defaultControlSettings));
            }
        }

        // player controls
        public KeyCode PrimaryForward { get; set; }
        public KeyCode SecondaryForward { get; set; }
        public KeyCode PrimaryLeft { get; set; }
        public KeyCode SecondaryLeft { get; set; }
        public KeyCode PrimaryBackward { get; set; }
        public KeyCode SecondaryBackward { get; set; }
        public KeyCode PrimaryRight { get; set; }
        public KeyCode SecondaryRight { get; set; }
        public KeyCode Interact { get; set; }
        public KeyCode ConfirmDialog { get; set; }
        public KeyCode CancelDialog { get; set; }
        public KeyCode ToggleMenu { get; set; }
        public KeyCode ToggleVoice { get; set; }
        public KeyCode OpenMap { get; set; }
        public KeyCode CallForHelp { get; set; }

        // master controls
        public KeyCode Sprint { get; set; }
        public KeyCode PushToTalkGlobal { get; set; }
        public KeyCode ToggleStats { get; set; }

        public void ReplaceNoneWithDefault(DefaultControlSettings defaultControlSettings)
        {
            var propertyInfos =
                typeof(ControlData).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var propertyInfo in propertyInfos)
                // KeyCodes
            {
                if (propertyInfo.GetValue(this) is KeyCode keyCode)
                {
                    if (keyCode == KeyCode.None)
                    {
                        propertyInfo.SetValue(this,
                            defaultControlSettings.GetType().GetProperty(propertyInfo.Name,
                                    BindingFlags.Instance | BindingFlags.Public)!
                                .GetValue(defaultControlSettings));
                    }
                }
                // Integers (sensitivity etc)
                else if (propertyInfo.GetValue(this) is int value)
                {
                    if (value == 0)
                    {
                        propertyInfo.SetValue(this,
                            defaultControlSettings.GetType().GetProperty(propertyInfo.Name,
                                    BindingFlags.Instance | BindingFlags.Public)!
                                .GetValue(defaultControlSettings));
                    }
                }
            }
        }
    }
}