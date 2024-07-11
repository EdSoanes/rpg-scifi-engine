using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Tests.Models
{
    public class Room : RpgEntity
    {
        [JsonProperty] public RpgContainer Contents { get; set; }

        public Room()
        {
            Contents = new RpgContainer(Id, nameof(Room));
        }
    }
}
