using Rpg.ModObjects.Behaviors;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.ModSets;
using Rpg.ModObjects.Server.Json;
using Rpg.ModObjects.States;
using System.Text.Json.Serialization;

namespace Rpg.ModObjects.Server.Tests
{
    public class PolymorphicSerializationTests
    {
        public class BuffState : State<Test1>
        {
            [JsonConstructor] public BuffState() { }

            public BuffState(Test1 owner)
                : base(owner)
            { }

            protected override bool IsOnWhen(Test1 owner)
                => owner.Value > 1;

            protected override void OnFillStateSet(StateModSet modSet, Test1 owner)
                => modSet.Add(owner, x => x.Bonus, 1);
        }

        public class NerfState : State<Test1>
        {
            [JsonConstructor] public NerfState() { }

            public NerfState(Test1 owner)
                : base(owner)
            { }

            protected override bool IsOnWhen(Test1 owner)
                => owner.Value < 1;

            protected override void OnFillStateSet(StateModSet modSet, Test1 owner)
                => modSet.Add(owner, x => x.Bonus, -1);
        }

        public class Test1 : RpgObject
        {
            public int Value { get; protected set; }
            public int Bonus { get; protected set; }

            public List<BaseBehavior> Behaviors { get; set; } = new();
            public List<State> States { get; set; } = new();
        }

        [Test]
        public void Polymorphic_Behaviors_Serialize()
        {
            var test = new Test1();
            test.Behaviors.Add(new Add());
            test.Behaviors.Add(new ExpiresOn(2));
            test.Behaviors.Add(new Threshold(0, 10));

            test.States.Add(new BuffState());
            test.States.Add(new NerfState());

            var json = RpgJson.Serialize(test);
            Assert.That(json, Is.Not.Null);

        }

        [Test]
        public void Polymorphic_Behaviors_Deserialize()
        {
            var test = new Test1();
            test.Behaviors.Add(new Add());
            test.Behaviors.Add(new ExpiresOn(2));
            test.Behaviors.Add(new Threshold(0, 10));

            test.States.Add(new BuffState());
            test.States.Add(new NerfState());

            var json = RpgJson.Serialize(test);
            var test2 = RpgJson.Deserialize<Test1>(json);

            Assert.That(test2, Is.Not.Null);

        }
    }
}

