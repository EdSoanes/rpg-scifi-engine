using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Core.Rules;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class StatPoints : Modifiable
    {
        public StatPoints() 
        {
            Name = nameof(StatPoints);
        }

        [JsonProperty] public virtual int BaseStrength { get; set; }
        [JsonProperty] public virtual int BaseDexterity { get; set; }
        [JsonProperty] public virtual int BaseIntelligence { get; set; }

        [Moddable] public virtual int Strength { get => BaseStrength + ModifierRoll(nameof(Strength)); }
        [Moddable] public virtual int Dexterity { get => BaseDexterity + ModifierRoll(nameof(Dexterity)); }
        [Moddable] public virtual int Intelligence { get => BaseIntelligence + ModifierRoll(nameof(Intelligence)); }

        [Moddable] public virtual int StrengthBonus { get => Stats.StatBonus(Strength) + ModifierRoll(nameof(StrengthBonus)); }
        [Moddable] public virtual int DexterityBonus { get => Stats.StatBonus(Strength) + ModifierRoll(nameof(DexterityBonus)); }
        [Moddable] public virtual int IntelligenceBonus { get => Stats.StatBonus(Strength) + ModifierRoll(nameof(IntelligenceBonus)); }

        [Moddable] public virtual int MissileAttackBonus { get => DexterityBonus + ModifierRoll(nameof(MissileAttackBonus)); }
        [Moddable] public virtual int MeleeAttackBonus { get => StrengthBonus + ModifierRoll(nameof(MeleeAttackBonus)); }
    }
}
