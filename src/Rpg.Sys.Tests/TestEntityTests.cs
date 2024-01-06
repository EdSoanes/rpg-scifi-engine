using Rpg.Sys.Modifiers;
using System.Reflection;

namespace Rpg.Sys.Tests
{
    public class TestEntityTests
    {
        private Graph _graph;
        private TestEntity _entity;

        [SetUp]
        public void Setup()
        {
            GraphExtensions.RegisterAssembly(Assembly.GetExecutingAssembly());
            _graph = new Graph();
            _entity = new TestEntity();

            _graph.Initialize(_entity);
        }

        [Test]
        public void TestEntity_EnsureSetup()
        {
            Assert.That(_graph.Entities.Count, Is.EqualTo(2));
            Assert.That(_graph.Mods.Count, Is.EqualTo(8));
        }

        [Test]
        public void TestEntity_AddStrengthBonusMod_EnsureStrengthBonusUpdate()
        {

            Assert.That(_entity.Strength, Is.EqualTo(16));
            Assert.That(_entity.StrengthBonus, Is.EqualTo(0));
            _graph.Mods.Add(BaseModifier.Create(_entity, x => x.Strength, x => x.StrengthBonus, () => DiceCalculations.CalculateStatBonus));

            Assert.That(_entity.StrengthBonus, Is.EqualTo(3));
        }

        [Test]
        public void TestEntity_AddStrengthBonusMod_EnsurePhysicalHealthUpdate()
        {
            Assert.That(_entity.Health.Physical, Is.EqualTo(10));

            _graph.Mods.Add(BaseModifier.Create(_entity, x => x.Strength, x => x.StrengthBonus, () => DiceCalculations.CalculateStatBonus));

            Assert.That(_entity.Health.Physical, Is.EqualTo(13));
        }

        [Test]
        public void TestEntity_AddStrengthBonusMod_EnsureNotifications()
        {
            var propNames = new List<string>();
            _entity.PropertyChanged += (s, e) => propNames.Add(e.PropertyName!);
            _entity.Health.PropertyChanged += (s, e) => propNames.Add($"{nameof(TestEntity.Health)}.{e.PropertyName!}");

            _graph.Mods.Add(BaseModifier.Create(_entity, x => x.Strength, x => x.StrengthBonus, () => DiceCalculations.CalculateStatBonus));

            Assert.That(propNames.Count, Is.EqualTo(2));
            Assert.That(propNames.Contains(nameof(TestEntity.StrengthBonus)), Is.True);
            Assert.That(propNames.Contains($"{nameof(TestEntity.Health)}.{nameof(TestHealth.Physical)}"), Is.True);
        }

        [Test]
        public void TestEntity_Serialize_EnsureValues()
        {
            _graph.Mods.Add(BaseModifier.Create(_entity, x => x.Strength, x => x.StrengthBonus, () => DiceCalculations.CalculateStatBonus));

            var json = _graph.Serialize<TestEntity>();
            var graph2 = Graph.Deserialize<TestEntity>(json);
            var entity2 = graph2.Context as TestEntity;

            Assert.That(entity2, Is.Not.Null);

            Assert.That(_entity.Strength, Is.EqualTo(16));
            Assert.That(_entity.StrengthBonus, Is.EqualTo(3));
            Assert.That(_entity.MeleeAttack, Is.EqualTo(10));
            Assert.That(_entity.MeleeDamage, Is.EqualTo(new Dice("1d6")));

            Assert.That(_entity.Health.Physical, Is.EqualTo(13));
            Assert.That(_entity.Health.Mental, Is.EqualTo(10));
        }

        [Test]
        public void TestEntity_PhysicalDamageMod_PhysicalHealingMod_Healed()
        {
            _graph.NewEncounter();

            _graph.Mods.Add(DamageModifier.Create(1, _entity, x => x.Health.Physical));

            var mods = _graph.Mods.GetMods(_entity, x => x.Health.Physical);
            Assert.That(mods, Is.Not.Null);
            Assert.That(mods.Count(), Is.EqualTo(3));

            Assert.That(_entity.Health.Physical, Is.EqualTo(9));

            _graph.NewTurn();
            Assert.That(_entity.Health.Physical, Is.EqualTo(9));

            _graph.EndEncounter();
            Assert.That(_entity.Health.Physical, Is.EqualTo(9));

            _graph.Mods.Add(HealingModifier.Create(1, _entity, x => x.Health.Physical));
            Assert.That(_entity.Health.Physical, Is.EqualTo(10));

            mods = _graph.Mods.GetMods(_entity, x => x.Health.Physical);

            Assert.That(mods, Is.Not.Null);
            Assert.That(mods.Count(), Is.EqualTo(2));
            Assert.That(mods.All(x => x.ModifierType == ModifierType.Base), Is.True);
        }

        [Test]
        public void TestEntity_ReplaceIntelligenceBaseWithBaseOverrideMod_VerifyValues()
        {
            Assert.That(_entity.Intelligence, Is.EqualTo(13));
            Assert.That(_entity.IntelligenceBonus, Is.EqualTo(1));

            _graph.Mods.Add(BaseOverrideModifier.Create(_entity, 18, x => x.Intelligence));

            Assert.That(_entity.Intelligence, Is.EqualTo(18));
            Assert.That(_entity.IntelligenceBonus, Is.EqualTo(4));

            _graph.Mods.Remove(_entity.Id, nameof(TestEntity.Intelligence), ModifierType.BaseOverride);

            Assert.That(_entity.Intelligence, Is.EqualTo(13));
            Assert.That(_entity.IntelligenceBonus, Is.EqualTo(1));
        }
    }
}