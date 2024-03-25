using UnityEngine;

namespace Settings
{
    public enum AvatarBody
    {
        Male,
        Female
    }

    public static class AvatarBodyExtensions
    {
        public static string GetDisplayName(this AvatarBody avatarBody)
        {
            return avatarBody switch
            {
                AvatarBody.Male => LocalizationUtility.GetLocalizedString("bodyMale"),
                AvatarBody.Female => LocalizationUtility.GetLocalizedString("bodyFemale"),
                _ => throw new UnityException($"Unknown avatar body {avatarBody}!")
            };
        }
    }
}