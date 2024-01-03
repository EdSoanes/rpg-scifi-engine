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

        [Moddable] public Dice BaseImpact { get => Evaluate(); }
        [Moddable] public Dice BasePierce { get => Evaluate(); }
        [Moddable] public Dice BaseBlast { get => Evaluate(); }
        [Moddable] public Dice BaseBurn { get => Evaluate(); }
        [Moddable] public Dice BaseEnergy { get => Evaluate(); }

        [Moddable] public Dice Impact { get => Evaluate(); }
        [Moddable] public Dice Pierce { get => Evaluate(); }
        [Moddable] public Dice Blast { get => Evaluate(); }
        [Moddable] public Dice Burn { get => Evaluate(); }
        [Moddable] public Dice Energy { get => Evaluate(); }

        public override Modifier[] Setup()
        {
            return new[]
            {
                BaseModifier.Create(this, _baseBlast, x => x.BaseBlast),
                BaseModifier.Create(this, _basePierce,x => x.BasePierce),
                BaseModifier.Create(this, _baseImpact, x => x.BaseImpact),
                BaseModifier.Create(this, _baseBurn, x => x.BaseBurn),
                BaseModifier.Create(this, _baseEnergy, x => x.BaseEnergy),

                BaseModifier.Create(this, x => x.BaseBlast, x => x.Blast),
                BaseModifier.Create(this, x => x.BasePierce, x => x.Pierce),
                BaseModifier.Create(this, x => x.BaseImpact, x => x.Impact),
                BaseModifier.Create(this, x => x.BaseBurn, x => x.Burn),
                BaseModifier.Create(this, x => x.BaseEnergy, x => x.Energy)
            };
        }
    }
}
