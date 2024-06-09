using Newtonsoft.Json;
using Rpg.ModObjects.Meta;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects.Tests.Models
{
    public class DamageValue : RpgComponent
    {
        [JsonProperty] public Dice Dice { get; protected set; }

        [PercentUI]
        [JsonProperty] public int ArmorPenetration { get; protected set; }

        [JsonProperty] 
        [MetersUI]
        public int Radius { get; protected set; }

        [JsonConstructor] private DamageValue() { }

        public DamageValue(string entityId, string name, Dice dice, int armorPenetration, int radius)
            : base(entityId, name)
        {
            Dice = dice;
            ArmorPenetration = armorPenetration;
            Radius = radius;
        }
    }
}
