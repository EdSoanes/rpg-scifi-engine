using Rpg.ModObjects;
using Rpg.ModObjects.Meta.Props;
using Rpg.ModObjects.Values;
using System.Text.Json.Serialization;

namespace Rpg.Cyborgs
{
    public class RangedWeapon : RpgEntity
    {
        [JsonInclude]
        [Dice]
        public Dice Damage { get; protected set; }

        [JsonInclude]
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
