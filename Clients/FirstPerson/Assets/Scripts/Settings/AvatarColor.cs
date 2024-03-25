using System.Collections.Generic;
using UnityEngine;

namespace Settings
{
    public enum AvatarColor
    {
        Black,
        White,
        Grey,
        Yellow,
        Orange,
        Red,
        Green,
        Blue,
        Purple,
        VeryLight,
        Light,
        Medium,
        Dark,
        VeryDark,
        Brown,
        Blonde,
        DarkBlonde,
    }

    public static class AvatarColorExtensions
    {
        public static readonly List<AvatarColor> HairColors = new List<AvatarColor>
        {
            AvatarColor.Brown, AvatarColor.Black, AvatarColor.Blonde, AvatarColor.DarkBlonde, AvatarColor.Red,
            AvatarColor.Grey
        };

        public static readonly List<AvatarColor> SkinColors = new List<AvatarColor>
        {
            AvatarColor.VeryLight, AvatarColor.Light, AvatarColor.Medium, AvatarColor.Dark, AvatarColor.VeryDark
        };

        public static readonly List<AvatarColor> MaskColors = new List<AvatarColor>
        {
            AvatarColor.Black, AvatarColor.White, AvatarColor.Grey
        };

        public static readonly List<AvatarColor> ShirtColors = new List<AvatarColor>
        {
            AvatarColor.Black, AvatarColor.White, AvatarColor.Grey, AvatarColor.Yellow, AvatarColor.Orange,
            AvatarColor.Red, AvatarColor.Green, AvatarColor.Blue, AvatarColor.Purple
        };

        public static readonly List<AvatarColor> TrouserColors = new List<AvatarColor>
        {
            AvatarColor.Black, AvatarColor.White, AvatarColor.Grey, AvatarColor.Yellow, AvatarColor.Orange,
            AvatarColor.Red, AvatarColor.Green, AvatarColor.Blue, AvatarColor.Purple
        };

        public static readonly List<AvatarColor> ShoeColors = new List<AvatarColor>
        {
            AvatarColor.Black, AvatarColor.White, AvatarColor.Grey
        };

        public static string GetDisplayName(this AvatarColor avatarColor)
        {
            return avatarColor switch
            {
                AvatarColor.Black => LocalizationUtility.GetLocalizedString("colorBlack"),
                AvatarColor.White => LocalizationUtility.GetLocalizedString("colorWhite"),
                AvatarColor.Grey => LocalizationUtility.GetLocalizedString("colorGrey"),
                AvatarColor.Yellow => LocalizationUtility.GetLocalizedString("colorYellow"),
                AvatarColor.Orange => LocalizationUtility.GetLocalizedString("colorOrange"),
                AvatarColor.Red => LocalizationUtility.GetLocalizedString("colorRed"),
                AvatarColor.Green => LocalizationUtility.GetLocalizedString("colorGreen"),
                AvatarColor.Blue => LocalizationUtility.GetLocalizedString("colorBlue"),
                AvatarColor.Purple => LocalizationUtility.GetLocalizedString("colorPurple"),
                AvatarColor.VeryLight => LocalizationUtility.GetLocalizedString("colorVeryLight"),
                AvatarColor.Light => LocalizationUtility.GetLocalizedString("colorLight"),
                AvatarColor.Medium => LocalizationUtility.GetLocalizedString("colorMedium"),
                AvatarColor.Dark => LocalizationUtility.GetLocalizedString("colorDark"),
                AvatarColor.VeryDark => LocalizationUtility.GetLocalizedString("colorVeryDark"),
                AvatarColor.Brown => LocalizationUtility.GetLocalizedString("colorBrown"),
                AvatarColor.Blonde => LocalizationUtility.GetLocalizedString("colorBlonde"),
                AvatarColor.DarkBlonde => LocalizationUtility.GetLocalizedString("colorDarkBlonde"),
                _ => throw new UnityException($"Unknown avatar color \"{avatarColor}\"!")
            };
        }

        public static Color GetColor(this AvatarColor avatarColor)
        {
            return avatarColor switch
            {
                AvatarColor.Black => GetColorForValues(28, 28, 28),
                AvatarColor.White => GetColorForValues(255, 255, 255),
                AvatarColor.Grey => GetColorForValues(119, 136, 153),
                AvatarColor.Yellow => GetColorForValues(255, 241, 108),
                AvatarColor.Orange => GetColorForValues(248, 119, 0),
                AvatarColor.Red => GetColorForValues(207, 43, 76),
                AvatarColor.Green => GetColorForValues(61, 140, 61),
                AvatarColor.Blue => GetColorForValues(70, 105, 180),
                AvatarColor.Purple => GetColorForValues(171, 130, 255),
                AvatarColor.VeryLight => GetColorForValues(255, 219, 172),
                AvatarColor.Light => GetColorForValues(241, 194, 125),
                AvatarColor.Medium => GetColorForValues(224, 172, 105),
                AvatarColor.Dark => GetColorForValues(198, 134, 66),
                AvatarColor.VeryDark => GetColorForValues(141, 85, 36),
                AvatarColor.Brown => GetColorForValues(79, 26, 0),
                AvatarColor.Blonde => GetColorForValues(241, 207, 119),
                AvatarColor.DarkBlonde => GetColorForValues(170, 136, 102),
                _ => throw new UnityException($"Unknown avatar color \"{avatarColor}\"!")
            };
        }

        private static Color GetColorForValues(int r, int g, int b)
        {
            return new Color(r / 255f, g / 255f, b / 255f);
        }
    }
}