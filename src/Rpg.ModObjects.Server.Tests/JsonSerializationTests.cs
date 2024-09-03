using Rpg.ModObjects.Props;
using Rpg.ModObjects.Server.Json;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;
using System.Text.Json.Serialization;

namespace Rpg.ModObjects.Server.Tests
{
    public class TestSpanOfTime
    {
        [JsonInclude] private bool Started { get; set; }
        [JsonInclude] public PointInTime Start { get; set; }
        [JsonInclude] public PointInTime End { get; set; }
    }

    public class JsonTestClass
    {
        public PointInTime Time { get; init; }
        public TestSpanOfTime Lifespan { get; init; }
        public PropRef PropRef { get; init; }
        public Dice DiceNumber { get; init; }
        public Dice Dice { get; init; }

        [JsonInclude] public string PrivateSet { get; private set; }
        [JsonInclude] private string PrivateGetSet { get; set; }
        [JsonConstructor] private JsonTestClass() { }

        public JsonTestClass(string ps)
        {
            PrivateSet = ps;
            PrivateGetSet = ps;
        }
    }

    public class Tests
    {
        [Test]
        public void TestClass_Serialize()
        {
            var time = new JsonTestClass("private_set")
            {
                Time = new PointInTime(PointInTimeType.BeforeTime),
                Lifespan = new TestSpanOfTime { Start = PointInTimeType.TimePassing, End = 3 },
                PropRef = new PropRef("1234abcd", "propertyName", RefType.Value),
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
                Lifespan = new TestSpanOfTime { Start = PointInTimeType.TimePassing, End = 3 },
                PropRef = new PropRef("1234abcd", "propertyName", RefType.Value),
                DiceNumber = 5,
                Dice = "1d6 + 2d8 +4"
            };

            var json = RpgJson.Serialize(time);

            var time2 = RpgJson.Deserialize<JsonTestClass>(json);

            Assert.That(time2, Is.Not.Null);
        }
    }
}