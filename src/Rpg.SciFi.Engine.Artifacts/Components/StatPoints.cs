using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.MetaData;

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
            this.AddBaseMod(x => x.BaseStrength, x => x.Strength);
            this.AddBaseMod(x => x.BaseDexterity, x => x.Dexterity);
            this.AddBaseMod(x => x.BaseIntelligence, x => x.Intelligence);

            this.AddBaseMod(x => x.Strength, x => x.StrengthBonus, () => Rules.CalculateStatBonus);
            this.AddBaseMod(x => x.Dexterity, x => x.DexterityBonus, () => Rules.CalculateStatBonus);
            this.AddBaseMod(x => x.Intelligence, x => x.IntelligenceBonus, () => Rules.CalculateStatBonus);

            this.AddBaseMod(x => x.StrengthBonus, x => x.MeleeAttackBonus);
            this.AddBaseMod(x => x.DexterityBonus, x => x.MissileAttackBonus);
        }
    }
}
