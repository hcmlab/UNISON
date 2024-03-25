using UnityEngine;

namespace Stations.Zones.Mall
{
    public enum Fund
    {
        VaccinationFund,
        Stocks
    }

    public static class FundExtensions
    {
        public static string GetDisplayName(this Fund fund)
        {
            return fund switch
            {
                Fund.VaccinationFund => LocalizationUtility.GetLocalizedString("vaccinationFund"),
                Fund.Stocks => LocalizationUtility.GetLocalizedString("stocks"),
                _ => throw new UnityException($"Unknown fund {fund}!")
            };
        }
    }
}