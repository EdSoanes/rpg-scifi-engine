using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Attributes;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class StatPoints : Modifiable
    {
        public StatPoints() 
        {
            Name = nameof(StatPoints);
        }

        [JsonProperty] public virtual int BaseStrength { get; protected set; }
        [JsonProperty] public virtual int BaseDexterity { get; protected set; }
        [JsonProperty] public virtual int BaseIntelligence { get; protected set; }

        [Modifiable] public virtual int Strength { get => BaseStrength + ModifierRoll(nameof(Strength)); }
        [Modifiable] public virtual int Dexterity { get => BaseDexterity + ModifierRoll(nameof(Dexterity)); }
        [Modifiable] public virtual int Intelligence { get => BaseIntelligence + ModifierRoll(nameof(Intelligence)); }
    }
}
