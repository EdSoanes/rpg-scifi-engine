using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Templates;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Tests.Models;
using Rpg.ModObjects.Tests.States;

namespace Rpg.ModObjects.Tests
{
    public class ModObjectTests
    {
        [SetUp]
        public void Setup()
        {
            RpgReflection.RegisterAssembly(this.GetType().Assembly);
        }

        [Test]
        public void TestEntity_EnsureSetup()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            Assert.That(graph.GetObjects().Count(), Is.EqualTo(3));
            var mods = graph.GetActiveMods();
            Assert.That(mods.Count(), Is.EqualTo(11));

            Assert.That(entity.States.Count(), Is.EqualTo(3));
            Assert.That(entity.GetState(nameof(BuffState)), Is.Not.Null);
            Assert.That(entity.GetState(nameof(NerfState)), Is.Not.Null);
            Assert.That(entity.GetState(nameof(Testing)), Is.Not.Null);
        }

        [Test]
        public void CreateSimpleEntity_SetBonus_VerifyScore()
        {
            var entity = new SimpleModdableEntity(2, 2);

            Assert.That(entity.Score, Is.EqualTo(2));
            Assert.That(entity.Bonus, Is.EqualTo(2));

            var graph = new RpgGraph(entity);

            Assert.That(entity.Score, Is.EqualTo(4));
            Assert.That(entity.Bonus, Is.EqualTo(2));
        }

        [Test]
        public void CreateSimpleEntity_AddScoreMod_EnsureScoreUpdate()
        {
            var entity = new SimpleModdableEntity(2, 2);
            var graph = new RpgGraph(entity);

            Assert.That(entity.Score, Is.EqualTo(4));

            entity.AddMod(new PermanentMod(), x => x.Score, 4);
            graph.Time.TriggerEvent();

            Assert.That(entity.Score, Is.EqualTo(8));
        }

        [Test]
        public void CreateModdableEntity_VerifyValues()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            Assert.That(entity.Strength.Score, Is.EqualTo(14));
            Assert.That(entity.Strength.Bonus, Is.EqualTo(2));
            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));
            Assert.That(entity.Damage.Dice.ToString(), Is.EqualTo("1d6 + 2"));
        }

        [Test]
        public void TestEntity_Serialize_EnsureValues()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            var json = RpgSerializer.Serialize(entity);

            var entity2 = RpgSerializer.Deserialize<ModdableEntity>(json)!;
            var graph2 = new RpgGraph(entity2);

            Assert.That(entity2, Is.Not.Null);

            Assert.That(entity2.Strength.Score, Is.EqualTo(entity.Strength.Score));
            Assert.That(entity2.Strength.Bonus, Is.EqualTo(entity.Strength.Bonus));

            Assert.That(entity2.Damage.ArmorPenetration, Is.EqualTo(entity.Damage.ArmorPenetration));
            Assert.That(entity2.Damage.Dice, Is.EqualTo(entity.Damage.Dice));
            Assert.That(entity2.Damage.Radius, Is.EqualTo(entity.Damage.Radius));

            Assert.That(entity2.Melee, Is.EqualTo(entity.Melee));
            Assert.That(entity2.Missile, Is.EqualTo(entity.Missile));
        }

        [Test]
        public void TestEntity_ReplaceStrengthScoreWithBaseOverrideMod_VerifyValues()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            Assert.That(entity.Strength.Score, Is.EqualTo(14));

            entity.AddMod(new OverrideMod(), x => x.Strength.Score, 10);
            graph.Time.TriggerEvent();

            Assert.That(entity.Strength.Score, Is.EqualTo(10));
            Assert.That(entity.Strength.Bonus, Is.EqualTo(0));
            Assert.That(entity.Melee.Roll(), Is.EqualTo(2));
            Assert.That(entity.Damage.Dice.ToString(), Is.EqualTo("1d6"));
        }

        [Test]
        public void TestEntity_CreateDamageMod_CreateRepairMod_IsRepaired()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            Assert.That(entity.Health, Is.EqualTo(10));
            Assert.That(graph.GetActiveMods().Count(), Is.EqualTo(11));

            entity.AddMod(new ExpireOnZeroMod(), x => x.Health, -10);
            graph.Time.TriggerEvent();

            Assert.That(entity.Health, Is.EqualTo(0));
            Assert.That(graph.GetActiveMods().Count(), Is.EqualTo(12));

            entity.AddMod(new ExpireOnZeroMod(), x => x.Health, 10);
            graph.Time.TriggerEvent();

            Assert.That(entity.Health, Is.EqualTo(10));
            Assert.That(graph.GetActiveMods().Count(), Is.EqualTo(11));
        }
    }
}
