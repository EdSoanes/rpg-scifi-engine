using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.MetaData;
using Rpg.SciFi.Engine.Artifacts.Modifiers;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class StatPoints : ModdableObject
    {
        [Moddable] public virtual int BaseStrength { get => Resolve(); }
        [Moddable] public virtual int BaseDexterity { get => Resolve(); }
        [Moddable] public virtual int BaseIntelligence { get => Resolve(); }

        [Moddable] public virtual int Strength { get => Resolve(); }
        [Moddable] public virtual int Dexterity { get => Resolve(); }
        [Moddable] public virtual int Intelligence { get => Resolve(); }

        [Moddable] public virtual int StrengthBonus { get => Resolve(); }
        [Moddable] public virtual int DexterityBonus { get => Resolve(); }
        [Moddable] public virtual int IntelligenceBonus { get => Resolve(); }

        [Moddable] public virtual int MissileAttackBonus { get => Resolve(); }
        [Moddable] public virtual int MeleeAttackBonus { get => Resolve(); }

        public override Modifier[] Setup()
        {
            return new[]
            {
                BaseModifier.Create(this, x => x.BaseStrength, x => x.Strength),
                BaseModifier.Create(this, x => x.BaseDexterity, x => x.Dexterity),
                BaseModifier.Create(this, x => x.BaseIntelligence, x => x.Intelligence),

                BaseModifier.Create(this, x => x.Strength, x => x.StrengthBonus, () => Rules.CalculateStatBonus),
                BaseModifier.Create(this, x => x.Dexterity, x => x.DexterityBonus, () => Rules.CalculateStatBonus),
                BaseModifier.Create(this, x => x.Intelligence, x => x.IntelligenceBonus, () => Rules.CalculateStatBonus),

                BaseModifier.Create(this, x => x.StrengthBonus, x => x.MeleeAttackBonus),
                BaseModifier.Create(this, x => x.DexterityBonus, x => x.MissileAttackBonus)
            };
        }
    }
}
