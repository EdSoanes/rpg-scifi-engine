using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Modifiers;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class StatPoints : Entity
    {
        [Moddable] public virtual int BaseStrength { get => this.Resolve(nameof(BaseStrength)); }
        [Moddable] public virtual int BaseDexterity { get => this.Resolve(nameof(BaseDexterity)); }
        [Moddable] public virtual int BaseIntelligence { get => this.Resolve(nameof(BaseIntelligence)); }

        [Moddable] public virtual int Strength { get => this.Resolve(nameof(Strength)); }
        [Moddable] public virtual int Dexterity { get => this.Resolve(nameof(Dexterity)); }
        [Moddable] public virtual int Intelligence { get => this.Resolve(nameof(Intelligence)); }

        [Moddable] public virtual int StrengthBonus { get => this.Resolve(nameof(StrengthBonus)); }
        [Moddable] public virtual int DexterityBonus { get => this.Resolve(nameof(DexterityBonus)); }
        [Moddable] public virtual int IntelligenceBonus { get => this.Resolve(nameof(IntelligenceBonus)); }

        [Moddable] public virtual int MissileAttackBonus { get => this.Resolve(nameof(MissileAttackBonus)); }
        [Moddable] public virtual int MeleeAttackBonus { get => this.Resolve(nameof(MeleeAttackBonus)); }

        [Setup]
        public Modifier[] Setup()
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
