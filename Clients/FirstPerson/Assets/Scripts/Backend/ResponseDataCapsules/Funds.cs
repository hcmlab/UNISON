using Newtonsoft.Json;

namespace Backend.ResponseDataCapsules
{
    public class Funds
    {
        public double VaccinationFund { get; }
        public double Stocks { get; }
        public double TaxAmount { get; }

        [JsonConstructor]
        public Funds(double vaccinationFund, double stocks, double taxAmount)
        {
            VaccinationFund = vaccinationFund;
            Stocks = stocks;
            TaxAmount = taxAmount;
        }
    }
}