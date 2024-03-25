using Newtonsoft.Json;

namespace Backend.ResponseDataCapsules
{
    public class HealthCheck
    {
        public double HealthPoints { get; }
        public double StressLevel { get; }
        public bool IsInfected { get; }

        [JsonConstructor]
        public HealthCheck(double healthPoints, double stressLevel, string isInfected)
        {
            HealthPoints = healthPoints;
            StressLevel = stressLevel;
            IsInfected = DeserializationUtility.StringToBool(isInfected);
        }
    }
}