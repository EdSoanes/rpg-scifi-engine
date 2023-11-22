using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class Movement : Modifiable
    {
        public Movement() 
        {
            Name = nameof(Movement);
        }
        public Movement(int baseSpeed, int baseAcceleration, int baseDeceleration, int baseManeuverability)
            : this()
        {
            BaseSpeed = baseSpeed;
            BaseAcceleration = baseAcceleration;
            BaseDeceleration = baseDeceleration;
            BaseManeuverability = baseManeuverability;
        }

        [JsonProperty] public int BaseSpeed { get; protected set; }
        [JsonProperty] public int BaseAcceleration { get; protected set; }
        [JsonProperty] public int BaseDeceleration { get; protected set; }
        [JsonProperty] public int BaseManeuverability { get; protected set; }

        [Moddable] public int Speed { get => BaseSpeed + ModifierRoll(nameof(Speed)); }
        [Moddable] public int Acceleration { get => BaseAcceleration + ModifierRoll(nameof(Acceleration)); }
        [Moddable] public int Deceleration { get => BaseDeceleration + ModifierRoll(nameof(Deceleration)); }
        [Moddable] public int Maneuverability { get => BaseManeuverability + ModifierRoll(nameof(Maneuverability)); }
    }
}

