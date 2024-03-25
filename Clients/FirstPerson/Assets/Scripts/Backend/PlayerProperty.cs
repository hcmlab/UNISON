using UnityEngine;

namespace Backend
{
    public enum PlayerProperty
    {
        IsInfected,

        StressLevel,
        EducationLevel,
        LearningSpeed,
        MoneyUnits,
        HealthPoints,

        Stocks,

        HasLearned,
        HasWorked,

        DaysInHospital,
        IsInQuarantine,

        BoughtCarePackage,
        BoughtDisinfectant,
        BoughtText,

        EveningActivity,
    }

    public static class PlayerPropertyExtensions
    {
        public static string GetNameForBackend(this PlayerProperty playerProperty)
        {
            return playerProperty switch
            {
                PlayerProperty.IsInfected => "isInfected",

                PlayerProperty.StressLevel => "stressLevel",
                PlayerProperty.EducationLevel => "educationLevel",
                PlayerProperty.LearningSpeed => "learningSpeed",
                PlayerProperty.MoneyUnits => "moneyUnits",
                PlayerProperty.HealthPoints => "healthPoints",

                PlayerProperty.Stocks => "stocks",

                PlayerProperty.HasLearned => "hasLearned",
                PlayerProperty.HasWorked => "hasWorked",

                PlayerProperty.DaysInHospital => "daysInHospital",
                PlayerProperty.IsInQuarantine => "isInQuarantine",

                PlayerProperty.BoughtCarePackage => "boughtCarePackage",
                PlayerProperty.BoughtDisinfectant => "boughtDisinfectant",
                PlayerProperty.BoughtText => "boughtText",

                PlayerProperty.EveningActivity => "eveningActivity",

                _ => throw new UnityException($"Unknown player property \"{playerProperty}\"!")
            };
        }
    }
}
