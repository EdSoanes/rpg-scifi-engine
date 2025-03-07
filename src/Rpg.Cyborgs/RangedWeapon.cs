using Newtonsoft.Json;
using Rpg.Experimental;
using Rpg.Experimental.System.Props;

namespace Rpg.Cyborgs
{
    public class RangedWeapon : RpgObject
    {
        [JsonProperty]
        [Dice]
        public Dice Damage { get; protected set; }

        [JsonProperty]
        [Integer]
        public int HitBonus { get; protected set; }

        [JsonConstructor] private RangedWeapon() { }

        public RangedWeapon(RangedWeaponTemplate template)
            : base(template.Name)
        {
            Damage = template.Damage;
            HitBonus = template.HitBonus;
        }
    }
}
