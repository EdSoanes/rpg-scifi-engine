using Newtonsoft.Json;

namespace Rpg.Experimental.Graph
{
    public sealed class RpgPropertyRef
    {
        [JsonProperty] public string ObjectId { get; private set; }
        [JsonProperty] public string Prop { get; private set; }

        [JsonConstructor] private RpgPropertyRef() { }

        public RpgPropertyRef(string objectId, string prop)
        {
            ObjectId = objectId;
            Prop = prop;
        }

        public override string ToString()
        {
            return $"{ObjectId}.{Prop}";
        }
    }
}
