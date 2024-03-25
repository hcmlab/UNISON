using UnityEngine;

namespace Backend
{
    public enum Scale
    {
        CostDisinfectant,
        CostHp,
        CostHealthCheck,
        OfficeFactor,
        CostSchool,
        InfectionProbability,
        HpLossEveryRound,
        StockDividend,
        GrowthPerStockInverse,
        IncomeTaxLevel,
        PropertyTaxLevel,
        HealthInsurance,
        SchoolCostFree,
        Lockdown,
        MinimumWage,
        Disappropriation,
        SocialSafety,
        HpLossDueToCovid,
        CostLounge,
        CostCarePackage,
        // New:
        StockIncomeTaxExempt,
        TaxRevenueForVaccine,
        HappierPlayers,
        TownHallAllowance,
    }

    public static class ScaleExtensions
    {
        public static string GetNameForBackend(this Scale scale)
        {
            return scale switch
            {
                Scale.CostDisinfectant => "costDisinfectant",
                Scale.CostHp => "costHp",
                Scale.CostHealthCheck => "costHealthCheck",
                Scale.OfficeFactor => "officeFactor",
                Scale.CostSchool => "costSchool",
                Scale.InfectionProbability => "infectionProbability",
                Scale.HpLossEveryRound => "hpLossEveryRound",
                Scale.StockDividend => "stockDividend",
                Scale.GrowthPerStockInverse => "growthPerStockInverse",
                Scale.IncomeTaxLevel => "incomeTaxLevel",
                Scale.PropertyTaxLevel => "propertyTaxLevel",
                Scale.HealthInsurance => "healthInsurance",
                Scale.SchoolCostFree => "schoolCostFree",
                Scale.Lockdown => "lockdown",
                Scale.MinimumWage => "minimumWage",
                Scale.Disappropriation => "disappropriation",
                Scale.SocialSafety => "socialSafety",
                Scale.HpLossDueToCovid => "hpLossDueToCovid",
                Scale.CostLounge => "costLounge",
                Scale.CostCarePackage => "costCarePackage",
                Scale.StockIncomeTaxExempt => "stockIncomeTaxExempt",
                Scale.TaxRevenueForVaccine => "taxRevenueForVaccine",
                Scale.HappierPlayers => "happierPlayers",
                Scale.TownHallAllowance => "townHallAllowance",
                _ => throw new UnityException($"Unknown scale \"{scale}\"!")
            };
        }
    }
}
