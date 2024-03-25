using Newtonsoft.Json;

namespace Backend.ResponseDataCapsules
{
    public class GameRound
    {
        [JsonConstructor]
        public GameRound(double vaccinationFund, double stocks, double taxAmount)
        {
            VaccinationFund = vaccinationFund;
            Stocks = stocks;
            TaxAmount = taxAmount;
        }

        public double VaccinationFund { get; }
        public double Stocks { get; }
        public double TaxAmount { get; }
    }
}