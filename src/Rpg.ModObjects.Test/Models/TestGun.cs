using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Tests.States;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects.Tests.Models
{
    /// <summary>
    /// HitBonus +2
    /// Damage.Dice d6
    /// Ammo 10
    /// </summary>

    public class TestGun : RpgEntity
    {
        public int HitBonus { get; private set; } = 2;
        public DamageValue Damage { get; private set; }
        public MaxCurrentValue Ammo { get; private set; }

        public TestGun() 
        { 
            Damage = new DamageValue(nameof(Damage), "d6", 0, 0);
            Ammo = new MaxCurrentValue(nameof(Ammo), 10);
        }
    }
}
