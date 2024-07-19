using Newtonsoft.Json;
using Rpg.ModObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
