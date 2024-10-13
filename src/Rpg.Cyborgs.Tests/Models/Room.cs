using Rpg.ModObjects;
using Newtonsoft.Json;

namespace Rpg.Cyborgs.Tests.Models
{
    public class Room : RpgEntity
    {
        [JsonProperty] public RpgContainer Contents { get; set; }

        public Room()
        {
            Contents = new RpgContainer(nameof(Room));
        }
    }
}
