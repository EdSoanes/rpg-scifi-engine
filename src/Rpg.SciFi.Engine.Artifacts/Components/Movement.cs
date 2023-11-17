using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class Movement
    {
        public Movement() { }
        public Movement(int baseSpeed, int baseAcceleration, int baseDeceleration, int baseManeuverability)
        {
            Speed = new Score(nameof(Speed), nameof(Speed), baseSpeed);
            Acceleration = new Score(nameof(Acceleration), nameof(Acceleration), baseAcceleration);
            Deceleration = new Score(nameof(Deceleration), nameof(Deceleration), baseDeceleration);
            Maneuverability = new Score(nameof(Maneuverability), nameof(Maneuverability), baseManeuverability);
        }

        [JsonProperty] public Score Speed { get; protected set; } = new Score(nameof(Speed), nameof(Speed), 0);
        [JsonProperty] public Score Acceleration { get; protected set; } = new Score(nameof(Acceleration), nameof(Acceleration), 0);
        [JsonProperty] public Score Deceleration { get; protected set; } = new Score(nameof(Deceleration), nameof(Deceleration), 0);
        [JsonProperty] public Score Maneuverability { get; protected set; } = new Score(nameof(Maneuverability), nameof(Maneuverability), 0);
    }
}
