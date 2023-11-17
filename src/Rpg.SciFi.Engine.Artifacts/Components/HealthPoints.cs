using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class HealthPoints
    {
        public HealthPoints() { }
        public HealthPoints(Score physical, Score mental)
        {
            Physical = physical;
            Mental = mental;
        }

        public HealthPoints(int physical, int mental)
        {
            Physical = new Score(nameof(Physical), nameof(Physical), physical);
            Mental = new Score(nameof(Mental), nameof(Mental), mental);
        }
        [JsonProperty] public virtual Score Physical { get; protected set; } = new Score(nameof(Physical));
        [JsonProperty] public virtual Score Mental { get; protected set; } = new Score(nameof(Mental));
    }
}
