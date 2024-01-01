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

        [Moddable] public int BaseSpeed { get => Resolve(); }
        [Moddable] public int BaseAcceleration { get => Resolve(); }
        [Moddable] public int BaseDeceleration { get => Resolve(); }
        [Moddable] public int BaseManeuverability { get => Resolve(); }

        [Moddable] public int Speed { get => Resolve(); }
        [Moddable] public int Acceleration { get => Resolve(); }
        [Moddable] public int Deceleration { get => Resolve(); }
        [Moddable] public int Maneuverability { get => Resolve(); }

        [Setup]
        public Modifier[] Setup()
        {
            return new[]
            {
                BaseModifier.Create(this, _baseSpeed, x => x.BaseSpeed),
                BaseModifier.Create(this, _baseAcceleration, x => x.BaseAcceleration),
                BaseModifier.Create(this, _baseDeceleration, x => x.BaseDeceleration),
                BaseModifier.Create(this, _baseManeuverability, x => x.BaseManeuverability),

                BaseModifier.Create(this, x => x.BaseSpeed, x => x.Speed),
                BaseModifier.Create(this, x => x.BaseAcceleration, x => x.Acceleration),
                BaseModifier.Create(this, x => x.BaseDeceleration, x => x.Deceleration),
                BaseModifier.Create(this, x => x.BaseManeuverability, x => x.Maneuverability)
            };
        }
    }
}

