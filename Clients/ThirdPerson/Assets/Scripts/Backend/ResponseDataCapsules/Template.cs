using Newtonsoft.Json;

namespace Backend.ResponseDataCapsules
{
    public class Template
    {
        [JsonConstructor]
        public Template(int id, string positiveText, string negativeText, string currentValue, string valueType,
            string scaleTitle, int? minimumValue, int? maximumValue, bool hasOpenPetition)
        {
            ID = id;
            PositiveText = positiveText;
            NegativeText = negativeText;
            CurrentValue = DeserializationUtility.IntOrBoolAsStringToInt(currentValue);
            ValueType = valueType;
            Scale = DeserializationUtility.GetScaleFromBackendName(scaleTitle);
            MinimumValue = minimumValue;
            MaximumValue = maximumValue;
            HasOpenPetition = hasOpenPetition;
        }

        public int ID { get; }
        public string PositiveText { get; }
        public string NegativeText { get; }
        public int CurrentValue { get; }
        public string ValueType { get; }
        public Scale Scale { get; }
        public int? MinimumValue { get; }
        public int? MaximumValue { get; }
        public bool HasOpenPetition { get; }
    }
}