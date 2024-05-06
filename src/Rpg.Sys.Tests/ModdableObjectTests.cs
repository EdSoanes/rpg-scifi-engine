using Rpg.Sys.Archetypes;
using Rpg.Sys.Components;
using Rpg.Sys.Components.Values;
using Rpg.Sys.Moddable;
using Rpg.Sys.Modifiers;
using Rpg.Sys.Tests.Factories;

namespace Rpg.Sys.Tests
{
    public class ModdableObjectTests
    {
        public class ModdableEntity : ModObject
        {
            public ScoreBonusValue Strength { get; private set; } = new ScoreBonusValue(nameof(Strength), 14);
            public DamageValue Damage { get; private set; } = new DamageValue("d6", 10, 100);
            public Dice Melee { get; protected set; } = 2;
            public Dice Missile { get; protected set; }

            protected override void OnInitialize()
            {
                PropStore.Init(this, BaseModifier.Create(this, x => x.Strength.Bonus, x => x.Melee));
                PropStore.Init(this, BaseModifier.Create(this, x => x.Strength.Bonus, x => x.Damage.Dice));
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

            protected override void OnInitialize()
            {
                PropStore.Init(this, BaseModifier.Create(this, x => x.Bonus, x => x.Score));
            }
        }

        [SetUp]
        public void Setup()
        {
            GraphExtensions.RegisterAssembly(this.GetType().Assembly);
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

            entity.AddBaseMod(4, x => x.Score);

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
    }
}
