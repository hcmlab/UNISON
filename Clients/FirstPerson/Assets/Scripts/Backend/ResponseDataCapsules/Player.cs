using System;
using System.Globalization;
using Newtonsoft.Json;

namespace Backend.ResponseDataCapsules
{
    public class Player
    {
        public int ID { get; }
        public string Name { get; }

        public bool IsIncubating { get; }
        public bool IsInfected { get; }

        public double StressLevel { get; }
        public double EducationLevel { get; }
        public double LearningSpeed { get; }
        public double MoneyUnits { get; }
        public double HealthPoints { get; }

        public double Stocks { get; }

        public bool HasLearned { get; }
        public bool HasWorked { get; }

        public int DaysInHospital { get; }
        public bool IsInQuarantine { get; }

        public bool BoughtCarePackage { get; }
        public bool BoughtDisinfectant { get; }
        public bool BoughtTest { get; }

        public string EveningActivity { get; }

        [JsonConstructor]
        public Player(int id, string name, string isIncubating, string isInfected, string stressLevel,
            string educationLevel, string learningSpeed, string moneyUnits, string healthPoints, string stocks,
            string hasLearned, string hasWorked, string daysInHospital, string isInQuarantine, string boughtCarePackage,
            string boughtDisinfectant, string boughtTest, string eveningActivity)
        {
            ID = id;
            Name = name;

            IsIncubating = DeserializationUtility.StringToBool(isIncubating);
            IsInfected = DeserializationUtility.StringToBool(isInfected);

            StressLevel = Convert.ToDouble(stressLevel, CultureInfo.InvariantCulture);
            EducationLevel = Convert.ToDouble(educationLevel, CultureInfo.InvariantCulture);
            LearningSpeed = Convert.ToDouble(learningSpeed, CultureInfo.InvariantCulture);
            MoneyUnits = Convert.ToDouble(moneyUnits, CultureInfo.InvariantCulture);
            HealthPoints = Convert.ToDouble(healthPoints, CultureInfo.InvariantCulture);

            Stocks = Convert.ToDouble(stocks, CultureInfo.InvariantCulture);

            HasLearned = DeserializationUtility.StringToBool(hasLearned);
            HasWorked = DeserializationUtility.StringToBool(hasWorked);

            DaysInHospital = Convert.ToInt32(daysInHospital, CultureInfo.InvariantCulture);
            IsInQuarantine = DeserializationUtility.StringToBool(isInQuarantine);

            BoughtCarePackage = DeserializationUtility.StringToBool(boughtCarePackage);
            BoughtDisinfectant = DeserializationUtility.StringToBool(boughtDisinfectant);
            BoughtTest = DeserializationUtility.StringToBool(boughtTest);

            EveningActivity = eveningActivity;
        }
    }
}
