using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Mods;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Core.Tests.Models
{
    public class TestWeapon : RpgEntity
    {
        public int HitBonus { get; set; } = 1;
        public Dice Damage { get; set; } = "1d6";

        [JsonConstructor] protected TestWeapon() { }

        public TestWeapon(string name)
            : base(name)
        {
        }
    }
}
