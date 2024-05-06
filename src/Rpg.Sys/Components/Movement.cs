using Newtonsoft.Json;
using Rpg.Sys.Components.Values;
using Rpg.Sys.Moddable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Components
{
    public class Movement : ModObject
    {
        [JsonProperty] public MaxCurrentValue Speed { get; private set; }
        [JsonProperty] public int Acceleration { get; protected set; }
        [JsonProperty] public int Deceleration { get; protected set; }

        [JsonConstructor] private Movement() { }

        public Movement(MovementTemplate template)
        {
            Speed = new MaxCurrentValue(nameof(Speed), template.MaxSpeed, 0);
            Acceleration = template.Acceleration;
            Deceleration = template.Deceleration;
        }

        public Movement(int maxSpeed, int acceleration, int deceleration)
        {
            Speed = new MaxCurrentValue(nameof(Speed), maxSpeed, 0);
            Acceleration = acceleration;
            Deceleration = deceleration;
        }
    }
}
