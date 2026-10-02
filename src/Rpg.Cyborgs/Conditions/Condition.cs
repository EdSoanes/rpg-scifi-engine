using Newtonsoft.Json;
using Rpg.Experimental;

namespace Rpg.Cyborgs.Conditions
{
    public abstract class Condition<T> : RpgState<T>
        where T : RpgObject
    {
        [JsonProperty] public string[] RemoveOnActions { get; init; } = [];

        [JsonConstructor] protected Condition()
            : base() { }

        public Condition(T owner)
            : base(owner)
        {
            Classification = "Condition";
        }
    }
}
