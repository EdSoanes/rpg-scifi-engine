using Newtonsoft.Json;
using Rpg.ModObjects.Cmds;
using Rpg.ModObjects.States;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects.Tests.Models
{
    public class DamageValue : ModObject
    {
        [JsonProperty] public Dice Dice { get; protected set; }
        [JsonProperty] public int ArmorPenetration { get; protected set; }
        [JsonProperty] public int Radius { get; protected set; }

        [JsonConstructor] private DamageValue() { }

        public DamageValue (Dice dice, int armorPenetration, int radius)
        {
            Dice = dice;
            ArmorPenetration = armorPenetration;
            Radius = radius;
        }
    }
}
