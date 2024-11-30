using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.ModSets;
using Rpg.ModObjects.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Core.Tests.Models
{
    public class WeaponDamaged : State<TestWeapon>
    {
        [JsonConstructor] private WeaponDamaged() { }

        public WeaponDamaged(TestWeapon owner)
            : base(owner)
        { }

        protected override void OnFillStateSet(StateModSet modSet, TestWeapon owner)
            => modSet.Add(owner, x => x.HitBonus, -1);
    }
}
