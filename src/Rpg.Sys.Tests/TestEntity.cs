using Rpg.Sys.Moddable;
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

        protected override void OnInitialize()
        {
            PropStore.Init(this, BaseModifier.Create(this, x => x.StrengthBonus, x => x.Health.Physical));
            PropStore.Init(this, BaseModifier.Create(this, x => x.Intelligence, x => x.IntelligenceBonus, () => DiceCalculations.CalculateStatBonus));
        }
    }

    public class TestHealth : ModObject
    {
        public int Physical { get; set; } = 10;
        public int Mental { get; set; } = 10;
    }
}
