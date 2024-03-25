﻿using Newtonsoft.Json;

namespace Backend.ResponseDataCapsules
{
    public class GameRound
    {
        public double VaccinationFund { get; }
        public double Stocks { get; }
        public double TaxAmount { get; }

        [JsonConstructor]
        public GameRound(double vaccinationFund, double stocks, double taxAmount)
        {
            VaccinationFund = vaccinationFund;
            Stocks = stocks;
            TaxAmount = taxAmount;
        }
    }
}
