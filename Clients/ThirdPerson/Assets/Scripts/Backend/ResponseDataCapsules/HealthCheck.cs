using Newtonsoft.Json;

namespace Backend.ResponseDataCapsules
{
    public class HealthCheck
    {
        [JsonConstructor]
        public HealthCheck(double healthPoints, double stressLevel, string isInfected)
        {
            HealthPoints = healthPoints;
            StressLevel = stressLevel;
            IsInfected = DeserializationUtility.StringToBool(isInfected);
        }

        public double HealthPoints { get; }
        public double StressLevel { get; }
        public bool IsInfected { get; }
    }
}