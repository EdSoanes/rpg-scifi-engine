using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Cyborgs.Tests.Models
{
    internal static class WeaponFactory
    {
        public static MeleeWeaponTemplate SwordTemplate 
        { 
            get => new MeleeWeaponTemplate
            {
                Name = "Excalibur",
                Damage = "d6",
                HitBonus = 1
            };
        }
    }
}
