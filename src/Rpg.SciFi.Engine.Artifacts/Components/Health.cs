using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Modifiers;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class Health : Entity
    {
        private readonly int _basePhysical;
        private readonly int _baseMental;

        [JsonConstructor] public Health() { }

        public Health(int basePhysical, int baseMental)
        {
            _basePhysical = basePhysical;
            _baseMental = baseMental;
        }

        [Moddable] public virtual int BasePhysical { get; protected set; }
        [Moddable] public virtual int BaseMental { get; protected set; }

        [Moddable] public virtual int Physical { get => this.Resolve(nameof(Physical)); }
        [Moddable] public virtual int Mental { get => this.Resolve(nameof(Mental)); }

        [Setup]
        public Modifier[] Setup()
        {
            return new[]
            {
                this.Mod(nameof(BasePhysical), _basePhysical, (x) => x.BasePhysical),
                this.Mod(nameof(BaseMental), _baseMental, (x) => x.BaseMental),

                this.Mod((x) => x.BasePhysical, (x) => x.Physical),
                this.Mod((x) => x.BaseMental, (x) => x.Mental)
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

        public override int BasePhysical { get => _healths.Sum(x => x.BasePhysical); }
        public override int BaseMental { get => _healths.Sum(x => x.BaseMental); }

        public override int Physical { get => _healths.Sum(x => x.Physical); }
        public override int Mental { get => _healths.Sum(x => x.Mental); }

    }
}
