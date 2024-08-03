using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.ModObjects.States;

namespace Rpg.Cyborgs.Conditions
{
    public abstract class Condition<T> : State<T>
        where T : RpgObject
    {
        [JsonProperty] public string[] RemoveOnActions { get; init; }

        protected Condition() { }
 
        public Condition(T owner)
            : base(owner) { }
    }
}
