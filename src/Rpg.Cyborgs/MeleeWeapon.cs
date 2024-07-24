using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.ModObjects.Meta.Props;
using Rpg.ModObjects.Values;

namespace Rpg.Cyborgs
{
    public class MeleeWeapon : RpgEntity
    {
        [JsonProperty]
        [Dice]
        public Dice Damage { get; protected set; }

        [JsonProperty]
        [Integer]
        public int HitBonus { get; protected set; }

        [JsonConstructor] private MeleeWeapon() { }

        public MeleeWeapon(MeleeWeaponTemplate template)
            : base(template.Name)
        {
            Damage = template.Damage;
            HitBonus = template.HitBonus;
        }
    }
}
