using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Modifiers;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class Health : ModdableObject
    {
        private readonly int _basePhysical;
        private readonly int _baseMental;

        [JsonConstructor] public Health() { }

        public Health(int basePhysical, int baseMental)
        {
            _basePhysical = basePhysical;
            _baseMental = baseMental;
        }

        [Moddable] public virtual int Physical { get => Resolve(); }
        [Moddable] public virtual int Mental { get => Resolve(); }

        public override Modifier[] Setup()
        {
            return new[]
            {
                BaseModifier.Create(this, _basePhysical, x => x.Physical),
                BaseModifier.Create(this, _baseMental, x => x.Mental)
            };
        }
    }

    public class CompositeHealth : Health
    {
        [JsonProperty] private Health[] _healths { get; set; }

        public CompositeHealth(Health[] healths)
        {
            _healths = healths;
        }

        public override int Physical { get => _healths.Sum(x => x.Physical); }
        public override int Mental { get => _healths.Sum(x => x.Mental); }

    }
}
