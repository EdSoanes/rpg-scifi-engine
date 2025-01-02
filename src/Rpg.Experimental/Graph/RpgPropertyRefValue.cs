//using Newtonsoft.Json;

//namespace Rpg.Experimental.Graph
//{
//    public class RpgPropertyRefValue
//    {
//        [JsonProperty] public RpgPropertyRef? PropertyRef { get; set; }
//        [JsonProperty] public object? Value { get; private set; }

//        [JsonConstructor] protected RpgPropertyRefValue() { }

//        public RpgPropertyRefValue(RpgPropertyRef? propertyRef, object? value)
//        {
//            PropertyRef = propertyRef;
//            Value = value;
//        }

//        public override string ToString()
//        {
//            return PropertyRef?.ToString() ?? Value?.ToString() ?? "null";
//        }
//    }
//}
