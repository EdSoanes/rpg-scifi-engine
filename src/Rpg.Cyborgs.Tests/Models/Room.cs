using Rpg.ModObjects;
using System.Text.Json.Serialization;

namespace Rpg.Cyborgs.Tests.Models
{
    public class Room : RpgEntity
    {
        [JsonInclude] public RpgContainer Contents { get; set; }

        public Room()
        {
            Contents = new RpgContainer(nameof(Room));
        }
    }
}
