using System;
using System.Globalization;
using Stations;
using UnityEngine;

namespace Backend
{
    public static class DeserializationUtility
    {
        public static int IntOrBoolAsStringToInt(string value)
        {
            return value switch
            {
                "" => 0,
                "false" => 0,
                "true" => 1,
                _ => Convert.ToInt32(value, CultureInfo.InvariantCulture)
            };
        }

        public static bool StringToBool(string value)
        {
            switch (value)
            {
                case "":
                case "0":
                case "false":
                    return false;

                case "1":
                case "true":
                    return true;

                default:
                    throw new UnityException($"Unknown bool value \"{value}\"!");
            }
        }

        public static StationType GetStationTypeFromBackendName(string stationType)
        {
            return stationType switch
            {
                "" => StationType.None,
                "city" => StationType.City,
                "mall" => StationType.Mall,
                "school" => StationType.School,
                "townHall" => StationType.TownHall,
                "hospital" => StationType.Hospital,
                "office" => StationType.Office,
                "lounge" => StationType.Lounge,
                "marketSquare" => StationType.MarketSquare,
                "home" => StationType.Home,
                _ => throw new UnityException($"Unknown station type \"{stationType}\"!")
            };
        }

        public static Scale GetScaleFromBackendName(string scale)
        {
            return scale switch
            {
                "costDisinfectant" => Scale.CostDisinfectant,
                "costHp" => Scale.CostHp,
                "costHealthCheck" => Scale.CostHealthCheck,
                "officeFactor" => Scale.OfficeFactor,
                "costSchool" => Scale.CostSchool,
                "infectionProbability" => Scale.InfectionProbability,
                "hpLossEveryRound" => Scale.HpLossEveryRound,
                "stockDividend" => Scale.StockDividend,
                "growthPerStockInverse" => Scale.GrowthPerStockInverse,
                "incomeTaxLevel" => Scale.IncomeTaxLevel,
                "propertyTaxLevel" => Scale.PropertyTaxLevel,
                "healthInsurance" => Scale.HealthInsurance,
                "schoolCostFree" => Scale.SchoolCostFree,
                "lockdown" => Scale.Lockdown,
                "minimumWage" => Scale.MinimumWage,
                "disappropriation" => Scale.Disappropriation,
                "socialSafety" => Scale.SocialSafety,
                "hpLossDueToCovid" => Scale.HpLossDueToCovid,
                "costLounge" => Scale.CostLounge,
                "costCarePackage" => Scale.CostCarePackage,
                "stockIncomeTaxExempt" => Scale.StockIncomeTaxExempt,
                "taxRevenueForVaccine" => Scale.TaxRevenueForVaccine,
                "happierPlayers" => Scale.HappierPlayers,
                "townHallAllowance" => Scale.TownHallAllowance,
                _ => throw new UnityException($"Unknown scale \"{scale}\"!")
            };
        }
    }
}