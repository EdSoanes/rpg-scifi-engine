using Rpg.ModObjects.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Cyborgs
{
    public class RangedWeaponTemplate
    {
        public string Name { get; set; }
        public Dice Damage { get; set; }
        public int HitBonus { get; set; }
    }
}
