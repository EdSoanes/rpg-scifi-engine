using Rpg.Sys.Modifiers;
using System.Reflection;

namespace Rpg.Sys.Tests
{
    public class TestEntityTests
    {
        private Graph _graph;

        [SetUp]
        public void Setup()
        {
            GraphExtensions.RegisterAssembly(Assembly.GetExecutingAssembly());
            _graph = new Graph();
        }

        [Test]
        public void CreateTestEntity_EnsureSetup()
        {
            _graph.Initialize(new TestEntity());
            Assert.That(_graph.Entities.Count, Is.EqualTo(2));
            Assert.That(_graph.Mods.Count, Is.EqualTo(6));
        }

        [Test]
        public void CreateTestEntity_AddStrengthBonusMod_EnsureStrengthBonusUpdate()
        {
            var entity = new TestEntity();
            _graph.Initialize(entity);

            Assert.That(entity.Strength, Is.EqualTo(16));
            Assert.That(entity.StrengthBonus, Is.EqualTo(0));
            _graph.Mods.Add(BaseModifier.Create(entity, x => x.Strength, x => x.StrengthBonus, () => DiceCalculations.CalculateStatBonus));

            Assert.That(entity.StrengthBonus, Is.EqualTo(3));
        }

        [Test]
        public void CreateTestEntity_AddStrengthBonusMod_EnsurePhysicalHealthUpdate()
        {
            var entity = new TestEntity();
            _graph.Initialize(entity);

            Assert.That(entity.Health.Physical, Is.EqualTo(10));

            _graph.Mods.Add(BaseModifier.Create(entity, x => x.Strength, x => x.StrengthBonus, () => DiceCalculations.CalculateStatBonus));

            Assert.That(entity.Health.Physical, Is.EqualTo(13));
        }

        [Test]
        public void CreateTestEntity_AddStrengthBonusMod_EnsureNotifications()
        {
            var entity = new TestEntity();
            _graph.Initialize(entity);

            var propNames = new List<string>();
            entity.PropertyChanged += (s, e) => propNames.Add(e.PropertyName!);
            entity.Health.PropertyChanged += (s, e) => propNames.Add($"{nameof(TestEntity.Health)}.{e.PropertyName!}");

            _graph.Mods.Add(BaseModifier.Create(entity, x => x.Strength, x => x.StrengthBonus, () => DiceCalculations.CalculateStatBonus));

            Assert.That(propNames.Count, Is.EqualTo(2));
            Assert.That(propNames.Contains(nameof(TestEntity.StrengthBonus)), Is.True);
            Assert.That(propNames.Contains($"{nameof(TestEntity.Health)}.{nameof(TestHealth.Physical)}"), Is.True);
        }

        [Test]
        public void CreateTestEntity_Serialize_EnsureValues()
        {
            var entity = new TestEntity();
            var graph = new Graph();

            graph.Initialize(entity);
            graph.Mods.Add(BaseModifier.Create(entity, x => x.Strength, x => x.StrengthBonus, () => DiceCalculations.CalculateStatBonus));

            var json = graph.Serialize<TestEntity>();
            var graph2 = Graph.Deserialize<TestEntity>(json);
            var entity2 = graph2.Context as TestEntity;

            Assert.That(entity2, Is.Not.Null);

            Assert.That(entity.Strength, Is.EqualTo(16));
            Assert.That(entity.StrengthBonus, Is.EqualTo(3));
            Assert.That(entity.MeleeAttack, Is.EqualTo(10));
            Assert.That(entity.MeleeDamage, Is.EqualTo(new Dice("1d6")));

            Assert.That(entity.Health.Physical, Is.EqualTo(13));
            Assert.That(entity.Health.Mental, Is.EqualTo(10));
        }
    }
}