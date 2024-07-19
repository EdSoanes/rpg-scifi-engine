using Newtonsoft.Json;
using Rpg.ModObjects.Meta.Props;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects.Tests.Models
{
    public class DamageValue : RpgComponent
    {
        [JsonProperty] 
        [Dice]
        public Dice Dice { get; protected set; }

        [JsonProperty]
        [Percent] 
        public int ArmorPenetration { get; protected set; }

        [JsonProperty] 
        [Meters]
        public int Radius { get; protected set; }

        [JsonConstructor] private DamageValue() { }

        public DamageValue(string name, Dice dice, int armorPenetration, int radius)
            : base(name)
        {
            Dice = dice;
            ArmorPenetration = armorPenetration;
            Radius = radius;
        }
    }
}
