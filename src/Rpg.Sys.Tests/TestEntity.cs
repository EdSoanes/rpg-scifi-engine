using Rpg.Sys.Modifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Tests
{
    public class TestEntity : ModdableObject
    {
        public int Strength { get; set; } = 16;
        public int StrengthBonus { get; set; }
        public int Intelligence { get; set; } = 13;
        public int IntelligenceBonus { get; set; }
        public Dice MeleeDamage { get; set; } = "1d6";
        public int MeleeAttack { get; set; } = 10;

        public TestHealth Health { get; set; } = new TestHealth();

        public override Modifier[] OnSetup()
        {
            var mods = new List<Modifier>(base.OnSetup())
            {
                BaseModifier.Create(this, x => x.StrengthBonus, x => x.Health.Physical),
                BaseModifier.Create(this, x => x.Intelligence, x => x.IntelligenceBonus, () => DiceCalculations.CalculateStatBonus)
            };

            return mods.ToArray();
        } 
    }

    public class TestHealth : ModdableObject
    {
        public int Physical { get; set; } = 10;
        public int Mental { get; set; } = 10;
    }
}
