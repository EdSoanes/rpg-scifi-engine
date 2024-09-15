using Rpg.ModObjects.Meta.Props;
using Rpg.ModObjects.Values;
using System.Text.Json.Serialization;

namespace Rpg.ModObjects.Tests.Models
{
    public class DamageValue : RpgComponent
    {
        [JsonInclude] 
        [Dice]
        public Dice Dice { get; protected set; }

        [JsonInclude]
        [Percent] 
        public int ArmorPenetration { get; protected set; }

        [JsonInclude] 
        [Meters]
        public int Radius { get; protected set; }

        [JsonConstructor] public DamageValue() : base() { }

        public DamageValue(string name, Dice dice, int armorPenetration, int radius)
            : base(name)
        {
            Dice = dice;
            ArmorPenetration = armorPenetration;
            Radius = radius;
        }
    }
}
