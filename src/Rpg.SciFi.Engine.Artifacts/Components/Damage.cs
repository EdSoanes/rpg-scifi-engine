using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.Modifiers;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class Damage : ModdableObject
    {
        private readonly Dice _baseImpact;
        private readonly Dice _basePierce;
        private readonly Dice _baseBlast;
        private readonly Dice _baseBurn;
        private readonly Dice _baseEnergy;

        [JsonConstructor] public Damage() { }

        public Damage(Dice baseImpact, Dice basePierce, Dice baseBlast, Dice baseBurn, Dice baseEnergy)
        {
            _baseImpact = baseImpact;
            _basePierce = basePierce;
            _baseBlast = baseBlast;
            _baseBurn = baseBurn;
            _baseEnergy = baseEnergy;
        }

        [Moddable] public Dice Impact { get => Evaluate(); }
        [Moddable] public Dice Pierce { get => Evaluate(); }
        [Moddable] public Dice Blast { get => Evaluate(); }
        [Moddable] public Dice Burn { get => Evaluate(); }
        [Moddable] public Dice Energy { get => Evaluate(); }

        public override Modifier[] Setup()
        {
            return new[]
            {
                BaseModifier.Create(this, _baseBlast, x => x.Blast),
                BaseModifier.Create(this, _basePierce,x => x.Pierce),
                BaseModifier.Create(this, _baseImpact, x => x.Impact),
                BaseModifier.Create(this, _baseBurn, x => x.Burn),
                BaseModifier.Create(this, _baseEnergy, x => x.Energy)
            };
        }
    }
}
