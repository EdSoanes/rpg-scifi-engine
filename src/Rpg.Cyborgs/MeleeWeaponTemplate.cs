using Rpg.ModObjects.Values;

namespace Rpg.Cyborgs
{
    public class MeleeWeaponTemplate
    {
        public string Name { get; set; }
        public Dice Damage { get; set; }
        public int HitBonus { get; set; }
    }
}
