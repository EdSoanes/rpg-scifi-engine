using Rpg.ModObjects.Time;
using Newtonsoft.Json;

namespace Rpg.ModObjects.Mods.ModSets
{
    public class StateModSet : ModSet
    {
        [JsonProperty] public string StateName { get; init; }

        [JsonConstructor] protected StateModSet()
            : base()
        { }

        public StateModSet(string ownerId, string stateName)
            : base(ownerId, stateName)
        {
            StateName = stateName;
        }

        protected override void CalculateExpiry()
        {
            Expiry = Graph.GetState(OwnerId, StateName)!.Expiry;
        }
    }
}
