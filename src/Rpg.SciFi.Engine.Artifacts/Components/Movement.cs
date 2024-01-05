using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Modifiers;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class Movement : ModdableObject
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

        [Moddable] public int Speed { get => Resolve(); }
        [Moddable] public int Acceleration { get => Resolve(); }
        [Moddable] public int Deceleration { get => Resolve(); }
        [Moddable] public int Maneuverability { get => Resolve(); }

        public override Modifier[] Setup()
        {
            return new[]
            {
                BaseModifier.Create(this, _baseSpeed, x => x.Speed),
                BaseModifier.Create(this, _baseAcceleration, x => x.Acceleration),
                BaseModifier.Create(this, _baseDeceleration, x => x.Deceleration),
                BaseModifier.Create(this, _baseManeuverability, x => x.Maneuverability)
            };
        }
    }
}

