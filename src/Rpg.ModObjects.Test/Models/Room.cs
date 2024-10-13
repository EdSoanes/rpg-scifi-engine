using Newtonsoft.Json;

namespace Rpg.ModObjects.Tests.Models
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
