using Newtonsoft.Json;
using Rpg.Experimental.Time;

namespace Rpg.Experimental.Graph
{
    public sealed class RpgObjectRef : Lifespan
    {
        [JsonProperty] public string ParentObjectId { get; private set; }
        [JsonProperty] public string ChildObjectId { get; private set; }

        public RpgObjectRef(string parentObjectId, string childObjectId, TimePoint start, TimePoint end)
            : base(start, end)
        {
            ParentObjectId = parentObjectId;
            ChildObjectId = childObjectId;
        }
    }
}
