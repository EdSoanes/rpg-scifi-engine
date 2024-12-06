using Newtonsoft.Json;

namespace Rpg.ModObjects.Mods.ModSets
{
    public class SyncedModSet : ModSet
    {
        [JsonProperty] public string SyncedToId { get; init; }
        [JsonConstructor] protected SyncedModSet()
            : base()
        { }

        public SyncedModSet(string ownerId, string syncedToId, string name)
            : base(ownerId, name)
        {
            SyncedToId = syncedToId;
        }

        protected override void CalculateExpiry()
            => CalculateExpiry(SyncedToId);
    }
}
