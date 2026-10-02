using Newtonsoft.Json;
using Rpg.Experimental;

namespace Rpg.Cyborgs.Tests.Models
{
    public class Room : RpgObject
    {
        [JsonProperty] public List<RpgObject> Contents { get; protected set; } = new();

        public Room()
            : base(nameof(Room))
        { }
    }
}
