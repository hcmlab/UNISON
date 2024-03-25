using UnityEngine;
using Random = System.Random;

namespace Settings
{
    public class AvatarSettings : MonoBehaviour
    {
        public static AvatarCustomization AvatarCustomization;

        private void Awake()
        {
            AvatarCustomization = LoadAvatarCustomization();
        }

        private AvatarCustomization LoadAvatarCustomization()
        {
            AvatarCustomization customization = LoadSave.LoadAvatarCustomizationFromFile();
            return customization ?? LoadDefaultAvatarCustomization();
        }

        private AvatarCustomization LoadDefaultAvatarCustomization()
        {
            Random rand = new Random();

            return new AvatarCustomization
            {
                Body = AvatarBody.Male,
                HairColor = AvatarColorExtensions.HairColors[rand.Next(AvatarColorExtensions.HairColors.Count)],
                SkinColor = AvatarColorExtensions.SkinColors[rand.Next(AvatarColorExtensions.SkinColors.Count)],
                MaskColor = AvatarColorExtensions.MaskColors[rand.Next(AvatarColorExtensions.MaskColors.Count)],
                ShirtColor = AvatarColorExtensions.ShirtColors[rand.Next(AvatarColorExtensions.ShirtColors.Count)],
                TrouserColor =
                    AvatarColorExtensions.TrouserColors[rand.Next(AvatarColorExtensions.TrouserColors.Count)],
                ShoeColor = AvatarColorExtensions.ShoeColors[rand.Next(AvatarColorExtensions.ShoeColors.Count)],
            };
        }
    }
}