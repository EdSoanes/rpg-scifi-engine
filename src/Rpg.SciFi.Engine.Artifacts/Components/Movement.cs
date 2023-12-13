using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Modifiers;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class Movement : Entity
    {
        private readonly int _baseSpeed;
        private readonly int _baseAcceleration;
        private readonly int _baseDeceleration;
        private readonly int _baseManeuverability;

        [JsonConstructor] private Movement() { }

        public Movement(int baseSpeed, int baseAcceleration, int baseDeceleration, int baseManeuverability)
        {
            _baseSpeed = baseSpeed;
            _baseAcceleration = baseAcceleration;
            _baseDeceleration = baseDeceleration;
            _baseManeuverability = baseManeuverability;
        }

        [Moddable] public int BaseSpeed { get => this.Resolve(nameof(BaseSpeed)); }
        [Moddable] public int BaseAcceleration { get => this.Resolve(nameof(BaseAcceleration)); }
        [Moddable] public int BaseDeceleration { get => this.Resolve(nameof(BaseDeceleration)); }
        [Moddable] public int BaseManeuverability { get => this.Resolve(nameof(BaseManeuverability)); }

        [Moddable] public int Speed { get => this.Resolve(nameof(Speed)); }
        [Moddable] public int Acceleration { get => this.Resolve(nameof(Acceleration)); }
        [Moddable] public int Deceleration { get => this.Resolve(nameof(Deceleration)); }
        [Moddable] public int Maneuverability { get => this.Resolve(nameof(Maneuverability)); }

        [Setup]
        public Modifier[] Setup()
        {
            return new[]
            {
                this.Mod(nameof(BaseSpeed), _baseSpeed, (x) => x.BaseSpeed),
                this.Mod(nameof(BaseAcceleration), _baseAcceleration, (x) => x.BaseAcceleration),
                this.Mod(nameof(BaseDeceleration), _baseDeceleration, (x) => x.BaseDeceleration),
                this.Mod(nameof(BaseManeuverability), _baseManeuverability, (x) => x.BaseManeuverability),

                this.Mod((x) => x.BaseSpeed, (x) => x.Speed),
                this.Mod((x) => x.BaseAcceleration, (x) => x.Acceleration),
                this.Mod((x) => x.BaseDeceleration, (x) => x.Deceleration),
                this.Mod((x) => x.BaseManeuverability, (x) => x.Maneuverability)
            };
        }
    }
}

