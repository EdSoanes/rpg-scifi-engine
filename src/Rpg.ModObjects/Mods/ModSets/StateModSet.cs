using Newtonsoft.Json;
using Rpg.ModObjects.States;

namespace Rpg.ModObjects.Mods.ModSets
{
    public class StateModSet : ModSet
    {
        [JsonProperty] public string StateName { get; init; }
        [JsonProperty] public StateInstanceType InstanceType { get; internal set; }

        [JsonConstructor] protected StateModSet()
            : base()
        { }

        public StateModSet(string ownerId, string stateName, StateInstanceType instanceType)
            : base(ownerId, stateName)
        {
            StateName = stateName;
            InstanceType = instanceType;
        }

        protected override void CalculateExpiry()
        {
            Expiry = Graph.GetState(OwnerId, StateName)!.Expiry;
        }
    }
}
