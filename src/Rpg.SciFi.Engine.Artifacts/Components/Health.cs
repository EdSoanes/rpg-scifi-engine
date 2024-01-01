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

        [Moddable] public virtual int BasePhysical { get => Resolve(); }
        [Moddable] public virtual int BaseMental { get => Resolve(); }

        [Moddable] public virtual int Physical { get => Resolve(); }
        [Moddable] public virtual int Mental { get => Resolve(); }

        [Setup]
        public Modifier[] Setup()
        {
            var mods = new List<Modifier>
            {
                BaseModifier.Create(this, x => x.BasePhysical, x => x.Physical),
                BaseModifier.Create(this, x => x.BaseMental, x => x.Mental)
            };

            if (_basePhysical > 0)
                mods.Add(BaseModifier.Create(this, _basePhysical, x => x.BasePhysical));

            if (_baseMental > 0)
                mods.Add(BaseModifier.Create(this, _baseMental, x => x.BaseMental));

            return mods.ToArray();
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
