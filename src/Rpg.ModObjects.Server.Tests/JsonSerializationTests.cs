using Rpg.ModObjects.Behaviors;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Server.Json;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;
using System.Text.Json.Serialization;

namespace Rpg.ModObjects.Server.Tests
{
    [JsonDerivedType(typeof(DerivedObj1), nameof(DerivedObj1))]
    [JsonDerivedType(typeof(DerivedObj2), nameof(DerivedObj2))]
    public abstract class BaseObj
    {
        public int BaseValue { get; init; }

        [JsonConstructor] public BaseObj() { }
    }

    public class DerivedObj1 : BaseObj
    {
        public string StrValue { get; init; }

        [JsonConstructor] public DerivedObj1() : base() { }
    }

    public class DerivedObj2 : BaseObj
    {
        public int DerivedValue { get; init; }

        [JsonConstructor] public DerivedObj2() : base() { }
    }

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

        [JsonInclude] public List<BaseObj> Objs { get; private set; } = new();

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

        [Test]
        public void TestClass_Behaviors_Deserialize()
        {
            var time = new JsonTestClass("private_set")
            {
                Time = new PointInTime(PointInTimeType.BeforeTime),
                Lifespan = new TestSpanOfTime { Start = PointInTimeType.TimePassing, End = 3 },
                PropRef = new PropRef("1234abcd", "propertyName", RefType.Value),
                DiceNumber = 5,
                Dice = "1d6 + 2d8 +4"
            };

            time.Objs.AddRange([
                new DerivedObj1 { BaseValue = 1, StrValue = "Obj1"},
                new DerivedObj2 { BaseValue = 2, DerivedValue = 3}
            ]);

            var json = RpgJson.Serialize(time);

            var time2 = RpgJson.Deserialize<JsonTestClass>(json);

            Assert.That(time2, Is.Not.Null);
            Assert.That(time2.Objs.Count, Is.EqualTo(2));
            Assert.That(time2.Objs.Any(x => x is DerivedObj1), Is.True);
            Assert.That(time2.Objs.First(x => x is DerivedObj1).BaseValue, Is.EqualTo(1));
            Assert.That((time2.Objs.First(x => x is DerivedObj1) as DerivedObj1)?.StrValue, Is.EqualTo("Obj1"));

            Assert.That(time2.Objs.Any(x => x is DerivedObj2), Is.True);
            Assert.That(time2.Objs.First(x => x is DerivedObj2).BaseValue, Is.EqualTo(2));
            Assert.That((time2.Objs.First(x => x is DerivedObj2) as DerivedObj2)?.DerivedValue, Is.EqualTo(3));
        }
    }
}