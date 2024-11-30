using Rpg.ModObjects.Mods.Mods;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Server.Json;
using Rpg.ModObjects.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Rpg.ModObjects;
using Rpg.Core.Tests.Models;

namespace Rpg.Core.Tests.Server
{
    public class BaseSerializationTests
    {
        private class DiceObj : RpgEntity
        {
            [JsonProperty] public Dice Dice { get; protected set; } = 2;
            public DiceObj(Dice dice)
                => Dice = dice;

            public Dice CalculateStatBonus(Dice dice) => (int)Math.Floor((double)(dice.Roll() - 10) / 2);
        }

        [SetUp]
        public void Setup()
        {
            RpgTypeScan.RegisterAssembly(this.GetType().Assembly);
            RpgTypeScan.RegisterAssembly(typeof(TestPerson).Assembly);
        }

        [Test]
        public void RpgArgs_Serialization()
        {

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
            var obj = new DiceObj(new Dice("2+2"));
            var graph = new RpgGraph(obj);

            var method = RpgMethodFactory.Create<DiceObj, Dice>(obj, nameof(DiceObj.CalculateStatBonus));

            Assert.That(method, Is.Not.Null);
            Assert.That(method.MethodName, Is.EqualTo(nameof(DiceObj.CalculateStatBonus)));
            Assert.That(method.ClassName, Is.Null);

            Assert.That(method.Args.Count(), Is.EqualTo(1));
            Assert.That(method.Args.First().Name, Is.EqualTo("dice"));

            var json = RpgJson.Serialize(method)!;
            var method2 = RpgJson.Deserialize<RpgMethod<DiceObj, Dice>>(json)!;
            Assert.That(method2, Is.Not.Null);
            Assert.That(method2.MethodName, Is.EqualTo(nameof(DiceObj.CalculateStatBonus)));
            Assert.That(method2.ClassName, Is.Null);

            Assert.That(method2.Args.Count(), Is.EqualTo(1));
            Assert.That(method2.Args.First().Name, Is.EqualTo("dice"));
        }


        [Test]
        public void BaseMod_Serialize_EnsureValues()
        {
            var obj = new DiceObj(new Dice("2+2"));
            var graph = new RpgGraph(obj);

            var baseMod = new Permanent()
                .Set(obj, x => x.Dice, 2, () => DiceCalculations.CalculateStatBonus);

            Assert.That(baseMod, Is.Not.Null);
            Assert.That(baseMod.Name, Is.EqualTo(nameof(DiceObj.Dice)));

            var json = RpgJson.Serialize(baseMod)!;
            var baseMod2 = RpgJson.Deserialize<Permanent>(json)!;

            Assert.That(baseMod2, Is.Not.Null);
            Assert.That(baseMod2.Name, Is.EqualTo(nameof(DiceObj.Dice)));
        }
    }
}
