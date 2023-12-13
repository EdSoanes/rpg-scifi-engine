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
                this.Mod((x) => x.BaseStrength, (x) => x.Strength),
                this.Mod((x) => x.BaseDexterity, (x) => x.Dexterity),
                this.Mod((x) => x.BaseIntelligence, (x) => x.Intelligence),

                this.Mod((x) => x.Strength, (x) => x.StrengthBonus, () => Rules.CalculateStatBonus),
                this.Mod((x) => x.Dexterity, (x) => x.DexterityBonus, () => Rules.CalculateStatBonus),
                this.Mod((x) => x.Intelligence, (x) => x.IntelligenceBonus, () => Rules.CalculateStatBonus),

                this.Mod((x) => x.StrengthBonus, (x) => x.MeleeAttackBonus),
                this.Mod((x) => x.DexterityBonus, (x) => x.MissileAttackBonus)
            };
        }
    }
}
