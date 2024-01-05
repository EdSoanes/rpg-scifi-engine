using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Modifiers;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class StatPoints : ModdableObject
    {
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
                BaseModifier.Create(this, x => x.Strength, x => x.StrengthBonus, () => Rules.CalculateStatBonus),
                BaseModifier.Create(this, x => x.Dexterity, x => x.DexterityBonus, () => Rules.CalculateStatBonus),
                BaseModifier.Create(this, x => x.Intelligence, x => x.IntelligenceBonus, () => Rules.CalculateStatBonus),

                BaseModifier.Create(this, x => x.StrengthBonus, x => x.MeleeAttackBonus),
                BaseModifier.Create(this, x => x.DexterityBonus, x => x.MissileAttackBonus)
            };
        }
    }
}
