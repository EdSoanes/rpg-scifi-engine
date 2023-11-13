using Rpg.SciFi.Engine.Artifacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Tests
{
    [TestClass]
    public class DamagesTests
    {
        [TestMethod]
        public void Damage_Serialization()
        {
            var damage = new DamageSignature();
            Assert.IsNotNull(damage);
                
            Assert.AreEqual("1d6", damage.Blast.Value);
            damage.Blast.AddModification(new Modification
            {
                Name = "Weapon Damage",
                Target = "Value",
                DiceExpr = "d8",
            });

            Assert.AreEqual("1d8 + 1d6", damage.Blast.Value);

            damage.Blast.ClearModifications();
            Assert.AreEqual("1d6", damage.Blast.Value);
        }
    }
}
