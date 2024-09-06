using Rpg.ModObjects.Mods.Mods;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Server.Json;
using Rpg.ModObjects.Tests.Models;
using Rpg.ModObjects.Values;
using System.Text.Json.Serialization;

namespace Rpg.ModObjects.Tests
{
    public class SerializationTests
    {
        private class DiceObj : RpgEntity
        {
            [JsonInclude] public Dice Dice { get; protected set; } = 2;
            public DiceObj(Dice dice)
                => Dice = dice;
        }

        [SetUp]
        public void Setup()
        {
            RpgTypeScan.RegisterAssembly(this.GetType().Assembly);
        }



        [Test]
        public void DiceWithDefault_Serialize_EnsureValues()
        {
            var obj = new DiceObj(new Dice("2+2"));

            var graph = new RpgGraph(obj);

            var json = RpgJson.Serialize(graph.GetGraphState());

            var graphState = RpgJson.Deserialize<RpgGraphState>(json)!;
            var graph2 = new RpgGraph(graphState);
            var obj2 = graph2.Context as DiceObj;

            Assert.That(obj2.Dice.Roll(), Is.EqualTo(4));
        }

        [Test]
        public void RpgMethod_Serialize_EnsureValues()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            var method = RpgMethod.Create<ScoreBonusValue, Dice>(entity.Strength, nameof(ScoreBonusValue.CalculateStatBonus));

            Assert.That(method, Is.Not.Null);
            Assert.That(method.MethodName, Is.EqualTo(nameof(ScoreBonusValue.CalculateStatBonus)));
            Assert.That(method.ClassName, Is.Null);

            Assert.That(method.Args.Count(), Is.EqualTo(1));
            Assert.That(method.Args.First().Name, Is.EqualTo("dice"));

            var json = RpgJson.Serialize(method)!;
            var method2 = RpgJson.Deserialize<RpgMethod<ScoreBonusValue, Dice>>(json)!;
            Assert.That(method2, Is.Not.Null);
            Assert.That(method2.MethodName, Is.EqualTo(nameof(ScoreBonusValue.CalculateStatBonus)));
            Assert.That(method2.ClassName, Is.Null);

            Assert.That(method2.Args.Count(), Is.EqualTo(1));
            Assert.That(method2.Args.First().Name, Is.EqualTo("dice"));
        }


        [Test]
        public void BaseMod_Serialize_EnsureValues()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            var method = RpgMethod.Create<ScoreBonusValue, Dice>(entity.Strength, nameof(ScoreBonusValue.CalculateStatBonus));

            var baseMod = new Permanent()
                .Set(entity.Strength, x => x.Bonus, 2, () => DiceCalculations.CalculateStatBonus);

            Assert.That(baseMod, Is.Not.Null);
            Assert.That(baseMod.Name, Is.EqualTo(nameof(ScoreBonusValue.Bonus)));

            var json = RpgJson.Serialize(baseMod)!;
            var baseMod2 = RpgJson.Deserialize<Permanent>(json)!;

            Assert.That(baseMod2, Is.Not.Null);
            Assert.That(baseMod2.Name, Is.EqualTo(nameof(ScoreBonusValue.Bonus)));
        }
    }
}
