using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts;
using Rpg.SciFi.Engine.Artifacts.Components;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Meta;

namespace Rpg.SciFi.Engine.Tests
{
    [TestClass]
    public class DamagesTests
    {
        internal class TestDamage : Damage
        {
            public TestDamage()
                : base("d6", "d6", "d10", "d6", "d6")
            {
            }
        }

        [TestMethod]
        public void StatPoints_Test()
        {
            var statPoints = new StatPoints();
            Meta.Initialize(statPoints);
            statPoints.Setup();

            Assert.IsNotNull(statPoints);
            Assert.AreEqual(-5, statPoints.StrengthBonus);

        }

        [TestMethod]
        public void Damage_TestDamage()
        {
            var damage = new Damage("d6", "d6", "d10", "d6", "d6");
            Meta.Initialize(damage);
            damage.Setup();

            Assert.IsNotNull(damage);
            Assert.AreEqual<string>("1d10", damage.Blast);
            Assert.AreEqual<string>("1d10", damage.BaseBlast);

            Assert.AreEqual<string>("1d6", damage.Burn);
            Assert.AreEqual<string>("1d6", damage.BaseBurn);
        }

        [TestMethod]
        public void Damage_Serialization()
        {
            var damage = new Damage("d6", "d6", "d10", "d6", "d6");
            Meta.Initialize(damage);
            damage.Setup();

            var state = Meta.Serialize();
            Meta.Clear();
            Assert.IsNull(Meta.Context);
            Assert.IsNull(Meta.MetaEntities);


            Meta.Deserialize<Damage>(state);

            Assert.IsNotNull(Meta.Context);
            Assert.IsNotNull(Meta.MetaEntities);

            var damage2 = (Damage)Meta.Context;
            Assert.AreEqual<string>(damage.Blast, damage2.Blast);
            Assert.AreEqual<string>(damage.BaseBlast, damage2.Blast);

            Assert.IsNotNull(damage2);
            Assert.AreEqual<string>(damage.Burn, damage2.Burn);
            Assert.AreEqual<string>(damage.BaseBurn, damage2.BaseBurn);

            Assert.IsNotNull(damage2);
            Assert.AreEqual<string>(damage.Energy, damage2.Energy);
            Assert.AreEqual<string>(damage.BaseEnergy, damage2.BaseEnergy);

            Assert.IsNotNull(damage2);
            Assert.AreEqual<string>(damage.Impact, damage2.Impact);
            Assert.AreEqual<string>(damage.BaseImpact, damage2.BaseImpact);

            Assert.IsNotNull(damage2);
            Assert.AreEqual<string>(damage.Pierce, damage2.Pierce);
            Assert.AreEqual<string>(damage.BasePierce, damage2.BasePierce);
        }

        [TestMethod]
        public void Damage_ApplyMod() 
        {
            var damage = new Damage("d6", "d6", "d6", "d6", "d6");
            Meta.Initialize(damage);
            damage.Setup();

            Assert.IsNotNull(damage);

            Assert.AreEqual<string>("1d6", damage.Blast);
            damage.AddMod(ModType.Instant, "Weapon Damage", "d8", x => x.Blast);

            Assert.AreEqual<string>("1d8 + 1d6", damage.Blast);

            damage.RemoveMods();

            Assert.AreEqual<string>("1d6", damage.Blast);
        }

        [TestMethod]
        public void Damage_Serialization_WithMod()
        {
            var damage = new Damage("d6", "d6", "d6", "d6", "d6");
            Meta.Initialize(damage);
            damage.Setup();

            Assert.IsNotNull(damage);

            damage.AddMod(ModType.Instant, "Weapon Damage", "d8", x => x.Blast);

            var state = Meta.Serialize();
            Meta.Clear();
            Meta.Deserialize<Damage>(state);

            var damage2 = (Damage)Meta.Context!;

            Assert.IsNotNull(damage2);
            Assert.AreEqual<string>("1d8 + 1d6", damage2.Blast);

            damage2.RemoveMods();
            Assert.AreEqual<string>("1d6", damage2.Blast);
        }
    }
}
