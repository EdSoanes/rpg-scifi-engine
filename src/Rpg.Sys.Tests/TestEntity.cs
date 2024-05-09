using Rpg.Sys.Moddable;
using Rpg.Sys.Moddable.Modifiers;
using Rpg.Sys.Modifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Tests
{
    public class TestEntity : ModObject
    {
        public int Strength { get; set; } = 16;
        public int StrengthBonus { get; set; }
        public int Intelligence { get; set; } = 13;
        public int IntelligenceBonus { get; set; }
        public Dice MeleeDamage { get; set; } = "1d6";
        public int MeleeAttack { get; set; } = 10;

        public TestHealth Health { get; set; } = new TestHealth();

        protected override void OnBuildGraph()
        {
            PropStore.Add(PermanentMod.Create(this, x => x.Health.Physical, x => x.StrengthBonus));
            PropStore.Add(PermanentMod.Create(this, x => x.IntelligenceBonus, x => x.Intelligence, () => DiceCalculations.CalculateStatBonus));
        }
    }

    public class TestHealth : ModObject
    {
        public int Physical { get; set; } = 10;
        public int Mental { get; set; } = 10;
    }
}
