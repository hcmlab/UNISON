using UnityEngine;

namespace Stations
{
    public enum StationType
    {
        None,
        City,
        Mall,
        School,
        TownHall,
        Hospital,
        Office,
        Lounge,
        MarketSquare,
        Home
    }

    public static class StationTypeExtensions
    {
        public static string GetDisplayName(this StationType stationType)
        {
            return stationType switch
            {
                StationType.None => "",
                StationType.City => LocalizationUtility.GetLocalizedString("city"),
                StationType.Mall => LocalizationUtility.GetLocalizedString("mall"),
                StationType.School => LocalizationUtility.GetLocalizedString("school"),
                StationType.TownHall => LocalizationUtility.GetLocalizedString("townHall"),
                StationType.Hospital => LocalizationUtility.GetLocalizedString("hospital"),
                StationType.Office => LocalizationUtility.GetLocalizedString("office"),
                StationType.Lounge => LocalizationUtility.GetLocalizedString("lounge"),
                StationType.MarketSquare => LocalizationUtility.GetLocalizedString("marketSquare"),
                StationType.Home => LocalizationUtility.GetLocalizedString("home"),
                _ => throw new UnityException($"Unknown station type \"{stationType}\"!")
            };
        }

        public static string GetNameForBackend(this StationType stationType)
        {
            return stationType switch
            {
                StationType.None => "",
                StationType.City => "city",
                StationType.Mall => "mall",
                StationType.School => "school",
                StationType.TownHall => "townHall",
                StationType.Hospital => "hospital",
                StationType.Office => "office",
                StationType.Lounge => "lounge",
                StationType.MarketSquare => "marketSquare",
                StationType.Home => "home",
                _ => throw new UnityException($"Unknown station type \"{stationType}\"!")
            };
        }
    }
}