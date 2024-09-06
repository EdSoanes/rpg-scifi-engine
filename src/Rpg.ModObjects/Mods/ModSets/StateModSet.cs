using Rpg.ModObjects.Time;
using System.Text.Json.Serialization;

namespace Rpg.ModObjects.Mods.ModSets
{
    public class StateModSet : ModSet
    {
        [JsonInclude] public string StateName { get; init; }
        [JsonInclude] public bool IsManual { get; private set; }

        [JsonConstructor] protected StateModSet()
            : base()
        { }

        public StateModSet(string ownerId, string stateName)
            : base(ownerId, stateName)
        {
            StateName = stateName;
        }

        internal void Update(bool isManual, LifecycleExpiry expiry)
        {
            IsManual = isManual;
            Expiry = expiry;
        }

        public override LifecycleExpiry OnStartLifecycle()
            => Graph.GetState(OwnerId, StateName)!.Expiry;

        public override LifecycleExpiry OnUpdateLifecycle()
            => Graph.GetState(OwnerId, StateName)!.Expiry;
    }
}
