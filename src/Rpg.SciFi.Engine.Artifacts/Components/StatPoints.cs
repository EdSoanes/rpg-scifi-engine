using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Modifiers;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class StatPoints : Entity
    {
        [JsonProperty] public virtual int BaseStrength { get; set; }
        [JsonProperty] public virtual int BaseDexterity { get; set; }
        [JsonProperty] public virtual int BaseIntelligence { get; set; }

        [Moddable] public virtual int Strength { get => this.Resolve(nameof(Strength)); }
        [Moddable] public virtual int Dexterity { get => this.Resolve(nameof(Dexterity)); }
        [Moddable] public virtual int Intelligence { get => this.Resolve(nameof(Intelligence)); }

        [Moddable] public virtual int StrengthBonus { get => this.Resolve(nameof(StrengthBonus)); }
        [Moddable] public virtual int DexterityBonus { get => this.Resolve(nameof(DexterityBonus)); }
        [Moddable] public virtual int IntelligenceBonus { get => this.Resolve(nameof(IntelligenceBonus)); }

        [Moddable] public virtual int MissileAttackBonus { get => this.Resolve(nameof(MissileAttackBonus)); }
        [Moddable] public virtual int MeleeAttackBonus { get => this.Resolve(nameof(MeleeAttackBonus)); }

        [Setup]
        public void Setup()
        {
            //TODO: Need rules calculator functionality here for bonuses
            this.Mod((x) => x.BaseStrength, (x) => x.Strength).IsBase().Apply();
            this.Mod((x) => x.BaseDexterity, (x) => x.Dexterity).IsBase().Apply();
            this.Mod((x) => x.BaseIntelligence, (x) => x.Intelligence).IsBase().Apply();

            this.Mod((x) => x.Strength, (x) => x.StrengthBonus, () => Rules.CalculateStatBonus).IsBase().Apply();
            this.Mod((x) => x.Dexterity, (x) => x.DexterityBonus, () => Rules.CalculateStatBonus).IsBase().Apply();
            this.Mod((x) => x.Intelligence, (x) => x.IntelligenceBonus, () => Rules.CalculateStatBonus).IsBase().Apply();

            this.Mod((x) => x.StrengthBonus, (x) => x.MeleeAttackBonus).IsBase().Apply();
            this.Mod((x) => x.DexterityBonus, (x) => x.MissileAttackBonus).IsBase().Apply();
        }
    }
}
