using Newtonsoft.Json;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects.Tests.Models
{
    public class DamageValue : RpgEntityComponent
    {
        [JsonProperty] public Dice Dice { get; protected set; }
        [JsonProperty] public int ArmorPenetration { get; protected set; }
        [JsonProperty] public int Radius { get; protected set; }

        [JsonConstructor] private DamageValue() { }

        public DamageValue (Guid entityId, string name, Dice dice, int armorPenetration, int radius)
            : base(entityId, name)
        {
            Dice = dice;
            ArmorPenetration = armorPenetration;
            Radius = radius;
        }
    }
}
