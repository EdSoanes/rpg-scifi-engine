using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.ModObjects.Meta.Attributes;
using Rpg.ModObjects.Values;

namespace Rpg.Cyborgs
{
    public abstract class MeleeWeapon : RpgEntity
    {
        [JsonProperty]
        [DiceUI]
        public Dice Damage { get; protected set; }

        [JsonProperty]
        [DiceUI]
        public int HitBonus { get; protected set; }
    }
}
