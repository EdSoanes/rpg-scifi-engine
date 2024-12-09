using Rpg.Core.Tests.Models;
using Rpg.ModObjects;
using Rpg.ModObjects.Description;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Mods;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Values;

namespace Rpg.Core.Tests
{
    public class DescriptionTests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeScan.RegisterAssembly(this.GetType().Assembly);
            RpgTypeScan.RegisterAssembly(typeof(TestPerson).Assembly);
        }

        [Test]
        public void DescribeMelee()
        {
            var person = new TestPerson("Benny");
            var graph = new RpgGraph(person);

            var desc = ObjectPropDescriber.Describe(graph, person, "MeleeAttack");
            Assert.That(desc, Is.Not.Null);

            Assert.That(desc.EntityId, Is.EqualTo(person.Id));
            Assert.That(desc.Archetype, Is.EqualTo(person.Archetype));
            Assert.That(desc.Name, Is.EqualTo(person.Name));
            Assert.That(desc.PropPath, Is.EqualTo("MeleeAttack"));
            Assert.That(desc.PropInfo.Value.Roll(), Is.EqualTo(2));
            Assert.That(desc.PropInfo.Mods.Count(), Is.EqualTo(2));

            var initialMod = desc.PropInfo.Mods.FirstOrDefault(x => x.ModType == nameof(Initial));

            Assert.That(initialMod, Is.Not.Null);
            Assert.That(initialMod.Value.Roll(), Is.EqualTo(1));

            var baseMods = desc.PropInfo.Mods.Where(x => x.ModType == nameof(Base));
            Assert.That(baseMods.Count(), Is.EqualTo(1));

            var baseMod = baseMods.First();
            Assert.That(baseMod.Source!.Prop, Is.EqualTo("StrengthBonus"));
            Assert.That(baseMod.Source!.Value.Roll(), Is.EqualTo(1));
            Assert.That(baseMod.Source!.Mods.Count(), Is.EqualTo(2));

            var scoreMod = baseMod.Source!.Mods.FirstOrDefault(x => x.Source?.Prop == "Strength");
            Assert.That(scoreMod, Is.Not.Null);
            Assert.That(scoreMod.Source!.Prop, Is.EqualTo("Strength"));
            Assert.That(scoreMod.Value.Roll(), Is.EqualTo(1));
            Assert.That(scoreMod.Source.Value.Roll(), Is.EqualTo(13));

        }

        [Test]
        public void DescribeModSet()
        {
            var person = new TestPerson("Benny");
            var graph = new RpgGraph(person);

            var modSet = new ModSet(person.Id, "Testing")
                .Add(person, x => x.MeleeAttack, 1)
                .Add(person, x => x.HitPoints, x => x.MeleeAttack);

            person.AddModSet(modSet);
            graph.Time.TriggerEvent();

            var desc = ObjectPropDescriber.Values(graph, modSet);

            Assert.That(desc, Is.Not.Null);
            Assert.That(desc.Name, Is.EqualTo("Testing"));
            Assert.That(desc.Values.Count(), Is.EqualTo(2));

            Assert.That(desc.Get(new PropRef(person.Id, "MeleeAttack")), Is.EqualTo(new Dice("1")));
            Assert.That(desc.Get(new PropRef(person.Id, "HitPoints")), Is.EqualTo(new Dice("3")));
        }
    }
}
