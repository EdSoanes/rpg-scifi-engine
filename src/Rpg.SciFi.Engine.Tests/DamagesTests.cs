using Rpg.SciFi.Engine.Artifacts.Components;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.MetaData;
using Rpg.SciFi.Engine.Artifacts;
using Rpg.SciFi.Engine.Artifacts.Modifiers;

namespace Rpg.SciFi.Engine.Tests
{
    [TestClass]
    public class DamagesTests
    {
        private Damage? _damage;
        private Meta<Damage>? _meta;

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
            Assert.IsNotNull(_damage);
            Assert.IsNotNull(_meta);

            var state = _meta.Serialize();

            var meta = Meta<Damage>.Deserialize(state);

            Assert.IsNotNull(meta);
            Assert.IsNotNull(meta.Context);

            var damage2 = meta.Context;
            var modProp = meta.GetModProp(damage2, x => x.Blast);
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
            Assert.IsNotNull(_meta);
            Assert.IsNotNull(_damage);

            _meta.AddMod(TimedModifier.Create(RemoveTurn.WhenZero, _damage, "Weapon Damage", "d8", x => x.Blast));

            var desc = _damage.Describe(x => x.Blast);

            Assert.AreEqual<string>("1d10 + 1d8", _damage!.Blast);

            _meta.ClearMods(_damage.Id);

            Assert.AreEqual<string>("1d10", _damage.Blast);
        }

        [TestMethod]
        public void Damage_Serialization_WithMod()
        {
            Assert.IsNotNull(_damage);
            Assert.IsNotNull(_meta);

            _damage.Name = "Something";

            _meta.AddMod(TimedModifier.Create(RemoveTurn.WhenZero, _damage, "Weapon Damage", "d8", x => x.Blast));

            var state = _meta.Serialize();

            _meta = Meta<Damage>.Deserialize(state);

            Assert.IsNotNull(_meta);
            Assert.IsNotNull(_meta.Context);
            var damage2 = _meta.Context!;

            Assert.IsNotNull(damage2);
            Assert.AreEqual("Something", damage2.Name);
            Assert.AreEqual<string>("1d10 + 1d8", damage2.Blast);

            _meta.ClearMods(damage2.Id);
            Assert.AreEqual<string>("1d10", damage2.Blast);
        }
    }
}
