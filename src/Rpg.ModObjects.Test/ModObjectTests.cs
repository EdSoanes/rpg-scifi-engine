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
            var graph = new ModGraph(entity);

            Assert.That(graph.GetEntities().Count(), Is.EqualTo(3));
            var mods = graph.GetAllMods();
            Assert.That(mods.Count(), Is.EqualTo(11));
            Assert.That(entity.StateNames.Count(), Is.EqualTo(2));
            Assert.That(entity.StateNames, Does.Contain("Buff"));
            Assert.That(entity.StateNames, Does.Contain("Nerf"));
            Assert.That(entity.ActiveStateNames.Count(), Is.EqualTo(0));
        }

        [Test]
        public void CreateSimpleEntity_SetBonus_VerifyScore()
        {
            var entity = new SimpleModdableEntity(2, 2);

            Assert.That(entity.Score, Is.EqualTo(2));
            Assert.That(entity.Bonus, Is.EqualTo(2));

            var graph = new ModGraph(entity);

            Assert.That(entity.Score, Is.EqualTo(4));
            Assert.That(entity.Bonus, Is.EqualTo(2));
        }

        [Test]
        public void CreateSimpleEntity_AddScoreMod_EnsureScoreUpdate()
        {
            var entity = new SimpleModdableEntity(2, 2);
            var graph = new ModGraph(entity);

            Assert.That(entity.Score, Is.EqualTo(4));

            entity.AddPermanentMod(x => x.Score, 4);
            entity.TriggerUpdate(x => x.Score);

            Assert.That(entity.Score, Is.EqualTo(8));
        }

        [Test]
        public void CreateModdableEntity_VerifyValues()
        {
            var entity = new ModdableEntity();
            var graph = new ModGraph(entity);

            Assert.That(entity.Strength.Score, Is.EqualTo(14));
            Assert.That(entity.Strength.Bonus, Is.EqualTo(2));
            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));
            Assert.That(entity.Damage.Dice.ToString(), Is.EqualTo("1d6 + 2"));
        }

        [Test]
        public void TestEntity_Serialize_EnsureValues()
        {
            var entity = new ModdableEntity();
            var graph = new ModGraph(entity);

            var json = JsonConvert.SerializeObject(entity, JsonSettings);

            var entity2 = JsonConvert.DeserializeObject<ModdableEntity>(json, JsonSettings)!;
            var graph2 = new ModGraph(entity2);

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
            var graph = new ModGraph(entity);

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
            var graph = new ModGraph(entity);

            Assert.That(entity.Health, Is.EqualTo(10));
            Assert.That(graph.GetAllMods().Count(), Is.EqualTo(11));

            entity.AddSumMod(x => x.Health, -10);
            entity.TriggerUpdate(x => x.Health);

            Assert.That(entity.Health, Is.EqualTo(0));
            Assert.That(graph.GetAllMods().Count(), Is.EqualTo(12));

            entity.AddSumMod(x => x.Health, 10);
            entity.TriggerUpdate(x => x.Health);

            Assert.That(entity.Health, Is.EqualTo(10));
            Assert.That(graph.GetAllMods().Count(), Is.EqualTo(11));
        }
    }
}
