using Rpg.ModObjects;
using Rpg.ModObjects.States;
using System.Text.Json.Serialization;

namespace Rpg.Cyborgs.Conditions
{
    public abstract class Condition<T> : State<T>
        where T : RpgObject
    {
        [JsonInclude] public string[] RemoveOnActions { get; init; }

        protected Condition() { }
 
        public Condition(T owner)
            : base(owner) { }
    }
}
