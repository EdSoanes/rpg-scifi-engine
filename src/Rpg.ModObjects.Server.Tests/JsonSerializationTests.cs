using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Mods;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Server.Json;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;
using Newtonsoft.Json;
using Rpg.Core.Tests.Models;

namespace Rpg.ModObjects.Server.Tests
{
    public class JsonTestClass : RpgEntity
    {
        public PointInTime Time { get; init; }
        public PropRef PropRef { get; init; }
        public Dice DiceNumber { get; init; }
        public Dice Dice { get; init; }

        public int Bonus { get; protected set; }

        [JsonProperty] public string PrivateSet { get; private set; }
        [JsonProperty] private string PrivateGetSet { get; set; }

        [JsonConstructor] private JsonTestClass() { }

        public JsonTestClass(string ps)
        {
            PrivateSet = ps;
            PrivateGetSet = ps;
        }
    }

    public class Tests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeScan.RegisterAssembly(this.GetType().Assembly);
            RpgTypeScan.RegisterAssembly(typeof(TestPerson).Assembly);
        }

        [Test]
        public void TestClass_Serialize()
        {
            var time = new JsonTestClass("private_set")
            {
                Time = new PointInTime(PointInTimeType.BeforeTime),
                PropRef = new PropRef("1234abcd", "propertyName"),
                DiceNumber = 5,
                Dice = "1d6 + 2d8 +4"
            };

            var json = RpgJson.Serialize(time);
            Assert.That(json, Is.Not.Null);
        }

        [Test]
        public void TestClass_Deserialize()
        {
            var time = new JsonTestClass("private_set")
            {
                Time = new PointInTime(PointInTimeType.BeforeTime),
                PropRef = new PropRef("1234abcd", "propertyName"),
                DiceNumber = 5,
                Dice = "1d6 + 2d8 +4"
            };

            var json = RpgJson.Serialize(time);

            var time2 = RpgJson.Deserialize<JsonTestClass>(json);

            Assert.That(time2, Is.Not.Null);
        }

        [Test]
        public void TestClass_Behaviors_Deserialize()
        {
            var time = new JsonTestClass("private_set")
            {
                Time = new PointInTime(PointInTimeType.BeforeTime),
                PropRef = new PropRef("1234abcd", "propertyName"),
                DiceNumber = 5,
                Dice = "1d6 + 2d8 +4"
            };
            var graph = new RpgGraph(time);
            time.AddMod(new Permanent(), x => x.Bonus, 1);
            time.AddMod(new Encounter(), x => x.Bonus, 1);

            var json = RpgJson.Serialize(time);

            var time2 = RpgJson.Deserialize<JsonTestClass>(json);

            Assert.That(time2, Is.Not.Null);
            Assert.That(time2.GetMods().Count, Is.EqualTo(5));
            Assert.That(time2.GetMods().Any(x => x is Permanent), Is.True);
            Assert.That(time2.GetMods().First(x => x is Permanent).SourceValue, Is.EqualTo(new Dice(1)));

            Assert.That(time2.GetMods().Any(x => x is Encounter), Is.True);
            Assert.That(time2.GetMods().First(x => x is Encounter).SourceValue, Is.EqualTo(new Dice(1)));
        }
    }
}