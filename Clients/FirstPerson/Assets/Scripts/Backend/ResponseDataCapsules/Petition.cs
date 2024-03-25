using System.Collections.Generic;
using Newtonsoft.Json;

namespace Backend.ResponseDataCapsules
{
    public class Petition
    {
        public int ID { get; }
        public string PositiveText { get; }
        public string NegativeText { get; }
        public int ProposedValue { get; }
        public int CurrentValue { get; }
        public string ValueType { get; }
        public Scale Scale { get; }
        public string Status { get; }
        public List<int> YesVotes { get; }
        public List<int> NoVotes { get; }

        [JsonConstructor]
        public Petition(int id, string positiveText, string negativeText, int proposedValue, int currentValue, string valueType, string scaleTitle, string status, List<int> yesVotes, List<int> noVotes)
        {
            ID = id;
            PositiveText = positiveText;
            NegativeText = negativeText;
            ProposedValue = proposedValue;
            CurrentValue = currentValue;
            ValueType = valueType;
            Scale = DeserializationUtility.GetScaleFromBackendName(scaleTitle);
            Status = status;
            YesVotes = yesVotes;
            NoVotes = noVotes;
        }
    }
}