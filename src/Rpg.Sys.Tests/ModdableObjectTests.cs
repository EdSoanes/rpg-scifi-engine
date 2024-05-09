using Newtonsoft.Json;
using Rpg.Sys.Components.Values;
using Rpg.Sys.Moddable;
using Rpg.Sys.Moddable.Modifiers;

namespace Rpg.Sys.Tests
{
    public class ModdableObjectTests
    {
        private static JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Include,
            Formatting = Formatting.Indented
        };

        public class ModdableEntity : ModObject
        {
            public ScoreBonusValue Strength { get; private set; } = new ScoreBonusValue(nameof(Strength), 14);
            public DamageValue Damage { get; private set; } = new DamageValue("d6", 10, 100);
            public Dice Melee { get; protected set; } = 2;
            public Dice Missile { get; protected set; }

            protected override void OnBuildGraph()
            {
                this.AddMod(x => x.Melee, x => x.Strength.Bonus);
                this.AddMod(x => x.Damage.Dice, x => x.Strength.Bonus);
            }
        }

        public class SimpleEntity : ModObject
        {
            public int Score { get; protected set; }
            public int Bonus { get; protected set; }

            public SimpleEntity(int score, int bonus) 
            {
                Score = score;
                Bonus = bonus;
            }

            protected override void OnBuildGraph()
            {
                this.AddMod(x => x.Score, x => x.Bonus);
            }
        }

        [SetUp]
        public void Setup()
        {
            GraphExtensions.RegisterAssembly(this.GetType().Assembly);
        }

        [Test]
        public void TestEntity_EnsureSetup()
        {
            var entity = new ModdableEntity();
            var graph = entity.BuildGraph();

            Assert.That(graph.GetEntities().Count(), Is.EqualTo(3));
            Assert.That(graph.GetMods().Count(), Is.EqualTo(8));
        }

        [Test]
        public void CreateSimpleEntity_SetBonus_VerifyScore()
        {
            var entity = new SimpleEntity(2, 2);

            Assert.That(entity.Score, Is.EqualTo(2));
            Assert.That(entity.Bonus, Is.EqualTo(2));

            entity.BuildGraph();

            Assert.That(entity.Score, Is.EqualTo(4));
            Assert.That(entity.Bonus, Is.EqualTo(2));
        }

        [Test]
        public void CreateSimpleEntity_AddScoreMod_EnsureScoreUpdate()
        {
            var entity = new SimpleEntity(2, 2);

            entity.BuildGraph();
            Assert.That(entity.Score, Is.EqualTo(4));

            entity.AddMod(x => x.Score, 4);

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

            entity.AddMod(BaseOverrideMod.Create(entity, x => x.Strength.Score, 10));
            entity.TriggerUpdate(x => x.Strength.Score);

            Assert.That(entity.Strength.Score, Is.EqualTo(10));
            Assert.That(entity.Strength.Bonus, Is.EqualTo(0));
            Assert.That(entity.Melee.Roll(), Is.EqualTo(2));
            Assert.That(entity.Damage.Dice.ToString(), Is.EqualTo("1d6"));
        }
    }
}
