using UnityEngine;

namespace Backend
{
    public enum GameProperty
    {
        VaccinationFund,
        Stocks,
        TaxAmount
    }

    public static class GamePropertyExtensions
    {
        public static string GetNameForBackend(this GameProperty gameProperty)
        {
            return gameProperty switch
            {
                GameProperty.VaccinationFund => "vaccinationFund",
                GameProperty.Stocks => "stocks",
                GameProperty.TaxAmount => "taxAmount",
                _ => throw new UnityException($"Unknown game property \"{gameProperty}\"!")
            };
        }
    }
}
