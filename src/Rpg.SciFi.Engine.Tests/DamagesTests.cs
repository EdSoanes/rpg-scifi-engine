using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts;
using Rpg.SciFi.Engine.Artifacts.Components;

namespace Rpg.SciFi.Engine.Tests
{
    [TestClass]
    public class DamagesTests
    {
        internal class TestDamageSignature : DamageSignature
        {
            public TestDamageSignature()
            {
                Blast = new Damage("TestBlast", "TestBlastDescription", "d10");
            }
        }

        [TestMethod]
        public void Damage_TestDamage()
        {
            var damage = new TestDamageSignature();

            Assert.IsNotNull(damage);
            Assert.AreEqual<string>("1d10", damage.Blast.Dice);
            Assert.AreEqual<string>("1d10", damage.Blast.BaseDice);
            Assert.AreEqual<string>("TestBlast", damage.Blast.Name);
            Assert.AreEqual<string>("TestBlastDescription", damage.Blast.Description!);

            Assert.AreEqual<string>("1d6", damage.Burn.Dice);
            Assert.AreEqual<string>("1d6", damage.Burn.BaseDice);
            Assert.AreEqual<string>("Burn", damage.Burn.Name);
            Assert.IsNull(damage.Burn.Description);
        }

        [TestMethod]
        public void Damage_Serialization()
        {
            var damage = new DamageSignature();

            var json = JsonConvert.SerializeObject(damage);
            var damage2 = JsonConvert.DeserializeObject<DamageSignature>(json);

            Assert.IsNotNull(damage2);
            Assert.AreEqual<string>(damage.Blast.Dice, damage2.Blast.Dice);
            Assert.AreEqual<string>(damage.Blast.BaseDice, damage2.Blast.BaseDice);

            Assert.IsNotNull(damage2);
            Assert.AreEqual<string>(damage.Burn.Dice, damage2.Burn.Dice);
            Assert.AreEqual<string>(damage.Burn.BaseDice, damage2.Burn.BaseDice);

            Assert.IsNotNull(damage2);
            Assert.AreEqual<string>(damage.Energy.Dice, damage2.Energy.Dice);
            Assert.AreEqual<string>(damage.Energy.BaseDice, damage2.Energy.BaseDice);

            Assert.IsNotNull(damage2);
            Assert.AreEqual<string>(damage.Impact.Dice, damage2.Impact.Dice);
            Assert.AreEqual<string>(damage.Impact.BaseDice, damage2.Impact.BaseDice);

            Assert.IsNotNull(damage2);
            Assert.AreEqual<string>(damage.Pierce.Dice, damage2.Pierce.Dice);
            Assert.AreEqual<string>(damage.Pierce.BaseDice, damage2.Pierce.BaseDice);
        }

        [TestMethod]
        public void Damage_ApplyMod() 
        {
            var damage = new DamageSignature();
            Assert.IsNotNull(damage);

            Assert.AreEqual<string>("1d6", damage.Blast.Dice);
            damage.Blast.AddModifier(new Modifier
            {
                Name = "Weapon Damage",
                Property = "Dice",
                Dice = "d8",
            });

            Assert.AreEqual<string>("1d8 + 1d6", damage.Blast.Dice);

            damage.Blast.ClearMods();

            Assert.AreEqual<string>("1d6", damage.Blast.Dice);
        }

        [TestMethod]
        public void Damage_Serialization_WithMod()
        {
            var damage = new DamageSignature();
            damage.Blast.AddModifier(new Modifier
            {
                Name = "Weapon Damage",
                Property = "Dice",
                Dice = "d8",
            });

            var json = JsonConvert.SerializeObject(damage);
            var damage2 = JsonConvert.DeserializeObject<DamageSignature>(json);

            Assert.IsNotNull(damage2);
            Assert.AreEqual<string>("1d8 + 1d6", damage2.Blast.Dice);

            damage2.Blast.ClearMods();
            Assert.AreEqual<string>("1d6", damage2.Blast.Dice);
        }
    }
}
