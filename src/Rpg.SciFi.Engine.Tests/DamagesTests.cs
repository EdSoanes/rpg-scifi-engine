using Rpg.SciFi.Engine.Artifacts.Components;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.MetaData;
using Rpg.SciFi.Engine.Artifacts;

namespace Rpg.SciFi.Engine.Tests
{
    [TestClass]
    public class DamagesTests
    {
        private Damage _damage;
        private Meta<Damage> _meta;

        [TestInitialize]
        public void Initialize()
        {
            _damage = new Damage("d6", "d6", "d10", "d6", "d6");
            _meta = new Meta<Damage>();
            _meta.Initialize(_damage);
        }
        [TestMethod]
        public void StatPoints_Test()
        {
            var statPoints = new StatPoints();
            var meta = new Meta<StatPoints>();
            meta.Initialize(statPoints);

            Assert.IsNotNull(statPoints);
            Assert.AreEqual(-5, statPoints.StrengthBonus);

        }

        [TestMethod]
        public void Damage_TestDamage()
        {
            Assert.IsNotNull(_damage);
            Assert.AreEqual<string>("1d10", _damage.Blast);
            Assert.AreEqual<string>("1d10", _damage.BaseBlast);

            Assert.AreEqual<string>("1d6", _damage.Burn);
            Assert.AreEqual<string>("1d6", _damage.BaseBurn);
        }

        [TestMethod]
        public void Damage_Serialization()
        {
            var state = _meta.Serialize();
            _meta.Clear();
            Assert.IsNull(_meta.Context);

            _meta.Deserialize(state);

            Assert.IsNotNull(_meta.Context);

            var damage2 = _meta.Context;
            Assert.AreEqual<string>(_damage.Blast, damage2.Blast);
            Assert.AreEqual<string>(_damage.BaseBlast, damage2.Blast);

            Assert.IsNotNull(damage2);
            Assert.AreEqual<string>(_damage.Burn, damage2.Burn);
            Assert.AreEqual<string>(_damage.BaseBurn, damage2.BaseBurn);

            Assert.IsNotNull(damage2);
            Assert.AreEqual<string>(_damage.Energy, damage2.Energy);
            Assert.AreEqual<string>(_damage.BaseEnergy, damage2.BaseEnergy);

            Assert.IsNotNull(damage2);
            Assert.AreEqual<string>(_damage.Impact, damage2.Impact);
            Assert.AreEqual<string>(_damage.BaseImpact, damage2.BaseImpact);

            Assert.IsNotNull(damage2);
            Assert.AreEqual<string>(_damage.Pierce, damage2.Pierce);
            Assert.AreEqual<string>(_damage.BasePierce, damage2.BasePierce);
        }

        [TestMethod]
        public void Damage_ApplyMod() 
        {
            Meta.Mods.Add(_damage.ModByPath<Dice>("Weapon Damage", "d8", nameof(_damage.Blast)).IsAdditive());

            Assert.AreEqual<string>("1d10 + 1d8", _damage.Blast);

            Meta.Mods.Clear(_damage.Id);

            Assert.AreEqual<string>("1d10", _damage.Blast);
        }

        [TestMethod]
        public void Damage_Serialization_WithMod()
        {
            _damage.Name = "Something";

            Meta.Mods.Add(_damage.ModByPath<Dice>("Weapon Damage", "d8", nameof(_damage.Blast)).IsAdditive());

            var state = _meta.Serialize();
            _meta.Clear();
            _meta.Deserialize(state);

            var damage2 = _meta.Context!;

            Assert.IsNotNull(damage2);
            Assert.AreEqual("Something", damage2.Name);
            Assert.AreEqual<string>("1d10 + 1d8", damage2.Blast);

            Meta.Mods.Clear(damage2.Id);
            Assert.AreEqual<string>("1d10", damage2.Blast);
        }
    }
}
