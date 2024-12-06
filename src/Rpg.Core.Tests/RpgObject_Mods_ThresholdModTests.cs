using Rpg.Core.Tests.Models;
using Rpg.ModObjects;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Mods;
using Rpg.ModObjects.Reflection;

namespace Rpg.Core.Tests
{
    public class RpgObject_Mods_ThresholdModTests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeScan.RegisterAssembly(this.GetType().Assembly);
            RpgTypeScan.RegisterAssembly(typeof(TestPerson).Assembly);
        }

        [Test]
        public void ApplyThresholdMod_EnsureSetup()
        {
            var person = new TestPerson("Benny");
            var graph = new RpgGraph(person);

            var mods = person.GetMods(nameof(TestPerson.Strength));

            Assert.That(mods.Count(), Is.EqualTo(2));
            Assert.That(mods.All(ModFilters.IsInitial), Is.True);

            var thresholdMod = mods.FirstOrDefault(x => x is Threshold);
            Assert.That(thresholdMod, Is.Not.Null);

            var threshold = thresholdMod as Threshold;
            Assert.That(threshold, Is.Not.Null);
            var behavior = threshold.Behavior as ModObjects.Behaviors.Threshold;
            Assert.That(behavior, Is.Not.Null);
            Assert.That(behavior.Min, Is.EqualTo(3));
            Assert.That(behavior.Max, Is.EqualTo(18));
        }

        [Test]
        public void SetValueBelowThreshold_ValueEqualsMin()
        {
            var person = new TestPerson("Benny");
            var graph = new RpgGraph(person);

            person.AddMod(new Permanent(), x => x.Strength, -200);
            graph.Time.TriggerEvent();

            Assert.That(person.Strength, Is.EqualTo(3));
            Assert.That(person.StrengthBonus, Is.EqualTo(-4));
        }

        [Test]
        public void SetMaxValueAboveThreshold_ValueEqualsMax()
        {
            var person = new TestPerson("Benny");
            var graph = new RpgGraph(person);

            person.AddMod(new Permanent(), x => x.Strength, 200);
            graph.Time.TriggerEvent();

            Assert.That(person.Strength, Is.EqualTo(18));
            Assert.That(person.StrengthBonus, Is.EqualTo(4));
        }

        [Test]
        public void SetMaxValueBelowThreshold_ValueLessThanMax()
        {
            var person = new TestPerson("Benny");
            var graph = new RpgGraph(person);

            person.AddMod(new Permanent(), x => x.Strength, 3);
            graph.Time.TriggerEvent();

            Assert.That(person.Strength, Is.EqualTo(16));
        }
    }
}
