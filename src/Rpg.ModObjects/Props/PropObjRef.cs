using Rpg.ModObjects.Time;
using Newtonsoft.Json;

namespace Rpg.ModObjects.Props
{
    public class PropObjRef : RpgLifecycleObject
    {
        [JsonProperty] public string EntityId { get; protected set; }
        [JsonProperty] public string? OwnerId { get; protected set; }

        public static bool operator ==(PropObjRef? d1, PropObjRef? d2) => d1?.OwnerId == d2?.OwnerId && d1?.EntityId == d2?.EntityId && d1?.Lifespan == d2?.Lifespan;
        public static bool operator !=(PropObjRef? d1, PropObjRef? d2) => d1?.OwnerId != d2?.OwnerId || d1?.EntityId != d2?.EntityId || d1?.Lifespan != d2?.Lifespan;

        public PropObjRef(string entityId, Lifespan lifespan, string? ownerId = null)
        {
            EntityId = entityId;
            Lifespan = lifespan;
            OwnerId = ownerId;
        }

        public override bool Equals(object? obj)
        {
            return obj is PropObjRef other 
                && other.OwnerId == OwnerId 
                && other.EntityId == EntityId 
                && other.Lifespan == Lifespan;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
