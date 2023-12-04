using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.MetaData;
using Rpg.SciFi.Engine.Artifacts.Modifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class Movement : Entity
    {
        public Movement(int baseSpeed, int baseAcceleration, int baseDeceleration, int baseManeuverability)
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

        [Moddable] public int Speed { get => this.Resolve(nameof(Speed)); }
        [Moddable] public int Acceleration { get => this.Resolve(nameof(Acceleration)); }
        [Moddable] public int Deceleration { get => this.Resolve(nameof(Deceleration)); }
        [Moddable] public int Maneuverability { get => this.Resolve(nameof(Maneuverability)); }

        [Setup]
        public void Setup()
        {
            this.Mod((x) => x.BaseSpeed, (x) => x.Speed).IsBase().Apply();
            this.Mod((x) => x.BaseAcceleration, (x) => x.Acceleration).IsBase().Apply();
            this.Mod((x) => x.BaseDeceleration, (x) => x.Deceleration).IsBase().Apply();
            this.Mod((x) => x.BaseManeuverability, (x) => x.Maneuverability).IsBase().Apply();
        }
    }
}

