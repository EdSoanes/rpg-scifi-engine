using Rpg.Core.Tests.Models;
using Rpg.ModObjects;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Mods;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Values;

namespace Rpg.Core.Tests
{
    public class RpgObject_Mods_OverrideModTests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeScan.RegisterAssembly(this.GetType().Assembly);
            RpgTypeScan.RegisterAssembly(typeof(TestPerson).Assembly);
        }

        [Test]
        public void OverrideBaseValue_EnsureSetup()
        {
            var person = new TestPerson("Benny");
            var graph = new RpgGraph(person);

            var mods = person.GetMods(nameof(TestPerson.Strength));

            Assert.That(mods.Count(), Is.EqualTo(2));

            person.OverrideBaseValue(nameof(TestPerson.Strength), 14);
            graph.Time.TriggerEvent();

            mods = person.GetMods(nameof(TestPerson.Strength));
            Assert.That(ModCalculator.BaseValue(graph, mods), Is.EqualTo(new Dice(14)));
        }

        [Test]
        public void OverrideBaseValue_RemoveOverride_EnsureValue()
        {
            var person = new TestPerson("Benny");
            var graph = new RpgGraph(person);

            person.OverrideBaseValue(nameof(TestPerson.Strength), 14);
            graph.Time.TriggerEvent();

            person.OverrideBaseValue(nameof(TestPerson.Strength), null);
            graph.Time.TriggerEvent();
            
            var mods = person.GetMods(nameof(TestPerson.Strength));
            Assert.That(ModCalculator.BaseValue(graph, mods), Is.EqualTo(new Dice(13)));
        }

        [Test]
        public void OverrideBaseValue_ReplaceOverride_EnsureValue()
        {
            var person = new TestPerson("Benny");
            var graph = new RpgGraph(person);

            person.OverrideBaseValue(nameof(TestPerson.Strength), 14);
            graph.Time.TriggerEvent();

            person.OverrideBaseValue(nameof(TestPerson.Strength), 15);
            graph.Time.TriggerEvent();

            var mods = person.GetMods(nameof(TestPerson.Strength));
            Assert.That(ModCalculator.BaseValue(graph, mods), Is.EqualTo(new Dice(15)));
        }

        [Test]
        public void OverrideBaseValue_HasAdditionalMod_EnsureValue()
        {
            var person = new TestPerson("Benny");
            var graph = new RpgGraph(person);

            graph.AddMods(new Permanent().Set(person, nameof(TestPerson.Strength), 1));
            graph.Time.TriggerEvent();

            var mods = person.GetMods(nameof(TestPerson.Strength));
            Assert.That(ModCalculator.Value(graph, mods), Is.EqualTo(new Dice(14)));

            person.OverrideBaseValue(nameof(TestPerson.Strength), 14);
            graph.Time.TriggerEvent();

            mods = person.GetMods(nameof(TestPerson.Strength));
            Assert.That(ModCalculator.BaseValue(graph, mods), Is.EqualTo(new Dice(14)));
            Assert.That(ModCalculator.Value(graph, mods), Is.EqualTo(new Dice(15)));
        }

        [Test]
        public void OverideBaseValue_EnsureThresholdValues()
        {
            var person = new TestPerson("Benny");
            var graph = new RpgGraph(person);

            Assert.That(person.ThresholdMinValue(nameof(TestPerson.Strength)), Is.EqualTo(3));
            Assert.That(person.ThresholdMaxValue(nameof(TestPerson.Strength)), Is.EqualTo(18));
        }

        [Test]
        public void OverideBaseValue_SetValueBelowThreshold_ValueBelowMin()
        {
            var person = new TestPerson("Benny");
            var graph = new RpgGraph(person);

            person.OverrideBaseValue(nameof(TestPerson.Strength), -14);
            var mods = person.GetMods(nameof(TestPerson.Strength));
            Assert.That(ModCalculator.Value(graph, mods), Is.EqualTo(new Dice(-14)));
        }
    }
}
