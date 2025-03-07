using Newtonsoft.Json;
using Rpg.Experimental;

namespace Rpg.Cyborgs
{
    public class MeleeWeapon : RpgObject
    {
        [JsonProperty]
        public Dice Damage { get; protected set; }

        [JsonProperty]
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
