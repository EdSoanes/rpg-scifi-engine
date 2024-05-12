using Newtonsoft.Json;
using Rpg.ModObjects.Modifiers;
using Rpg.ModObjects.Tests.Models;

namespace Rpg.ModObjects.Tests
{
    public class ModObjectTests
    {
        private static JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Include,
            Formatting = Formatting.Indented
        };

        [SetUp]
        public void Setup()
        {
            ModGraphExtensions.RegisterAssembly(this.GetType().Assembly);
        }

        [Test]
        public void TestEntity_EnsureSetup()
        {
            var entity = new ModdableEntity();
            var graph = entity.BuildGraph();

            Assert.That(graph.GetEntities().Count(), Is.EqualTo(3));
            Assert.That(graph.GetMods().Count(), Is.EqualTo(9));
        }

        [Test]
        public void CreateSimpleEntity_SetBonus_VerifyScore()
        {
            var entity = new SimpleModdableEntity(2, 2);

            Assert.That(entity.Score, Is.EqualTo(2));
            Assert.That(entity.Bonus, Is.EqualTo(2));

            entity.BuildGraph();

            Assert.That(entity.Score, Is.EqualTo(4));
            Assert.That(entity.Bonus, Is.EqualTo(2));
        }

        [Test]
        public void CreateSimpleEntity_AddScoreMod_EnsureScoreUpdate()
        {
            var entity = new SimpleModdableEntity(2, 2);

            entity.BuildGraph();
            Assert.That(entity.Score, Is.EqualTo(4));

            entity.AddPermanentMod(x => x.Score, 4);
            entity.TriggerUpdate(x => x.Score);

            Assert.That(entity.Score, Is.EqualTo(8));
        }

        [Test]
        public void CreateModdableEntity_VerifyValues()
        {
            var entity = new ModdableEntity();
            entity.BuildGraph();

            Assert.That(entity.Strength.Score, Is.EqualTo(14));
            Assert.That(entity.Strength.Bonus, Is.EqualTo(2));
            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));
            Assert.That(entity.Damage.Dice.ToString(), Is.EqualTo("1d6 + 2"));
        }

        [Test]
        public void TestEntity_Serialize_EnsureValues()
        {
            var entity = new ModdableEntity();
            entity.BuildGraph();

            var json = JsonConvert.SerializeObject(entity, JsonSettings);

            var entity2 = JsonConvert.DeserializeObject<ModdableEntity>(json, JsonSettings)!;
            entity2.BuildGraph();

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
            entity.BuildGraph();

            Assert.That(entity.Strength.Score, Is.EqualTo(14));

            entity.AddBaseOverrideMod(x => x.Strength.Score, 10);
            entity.TriggerUpdate(x => x.Strength.Score);

            Assert.That(entity.Strength.Score, Is.EqualTo(10));
            Assert.That(entity.Strength.Bonus, Is.EqualTo(0));
            Assert.That(entity.Melee.Roll(), Is.EqualTo(2));
            Assert.That(entity.Damage.Dice.ToString(), Is.EqualTo("1d6"));
        }

        [Test]
        public void TestEntity_CreateDamageMod_CreateRepairMod_IsRepaired()
        {
            var entity = new ModdableEntity();
            var graph = entity.BuildGraph();

            Assert.That(entity.Health, Is.EqualTo(10));
            Assert.That(graph.GetMods().Count(), Is.EqualTo(9));

            entity.AddDamageMod(x => x.Health, 10);
            entity.TriggerUpdate(x => x.Health);

            Assert.That(entity.Health, Is.EqualTo(0));
            Assert.That(graph.GetMods().Count(), Is.EqualTo(10));

            entity.AddRepairMod(x => x.Health, 10);
            entity.TriggerUpdate(x => x.Health);

            Assert.That(entity.Health, Is.EqualTo(10));
            Assert.That(graph.GetMods().Count(), Is.EqualTo(9));
        }
    }
}
