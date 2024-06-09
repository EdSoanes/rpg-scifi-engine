using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.ModObjects.Meta;
using Rpg.ModObjects.Values;

namespace Rpg.Sys.Components.Values
{
    public class DamageValue : RpgComponent
    {
        [JsonProperty] public Dice Dice { get; protected set; }
        [JsonProperty] public int ArmorPenetration { get; protected set; }
        [JsonProperty] public int Radius { get; protected set; }

        [JsonConstructor] private DamageValue() { }

        public DamageValue (string entityId, string name, Dice dice, int armorPenetration, int radius)
            : base(entityId, name)
        {
            Dice = dice;
            ArmorPenetration = armorPenetration;
            Radius = radius;
        }
    }
}
