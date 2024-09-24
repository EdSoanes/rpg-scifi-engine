using Rpg.ModObjects.Time;
using System.Text.Json.Serialization;

namespace Rpg.ModObjects.Mods.ModSets
{
    public class StateModSet : ModSet
    {
        [JsonInclude] public string StateName { get; init; }

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
