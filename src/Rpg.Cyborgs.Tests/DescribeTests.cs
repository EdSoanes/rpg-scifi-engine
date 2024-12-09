using Rpg.Cyborgs.Tests.Models;
using Rpg.ModObjects;
using Rpg.ModObjects.Description;
using Rpg.ModObjects.Mods.Mods;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Reflection;

namespace Rpg.Cyborgs.Tests
{
    public class DescribeTests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeScan.RegisterAssembly(typeof(CyborgsSystem).Assembly);
        }

        [Test]
        public void Benny_DescribeDefence_EnsureValues()
        {
            var pc = new PlayerCharacter(ActorFactory.BennyTemplate);
            var graph = new RpgGraph(pc);

            var propDesc = ObjectPropDescriber.Describe(graph, pc, "Defence.Value");
            Assert.That(propDesc, Is.Not.Null);

            Assert.That(propDesc.PropInfo.Value.ToString, Is.EqualTo("7"));
            Assert.That(propDesc.Archetype, Is.EqualTo("PlayerCharacter"));
            Assert.That(propDesc.PropPath, Is.EqualTo("Defence.Value"));

            Assert.That(propDesc.Archetype, Is.EqualTo("PropValue"));
            Assert.That(propDesc.PropPath, Is.EqualTo("Value"));

            Assert.That(propDesc.PropInfo.Mods.Count(), Is.EqualTo(2));
            Assert.That(propDesc.PropInfo.Mods.Count(x => x.ModType == nameof(Initial) || x.ModType == nameof(Threshold)), Is.EqualTo(1));
            Assert.That(propDesc.PropInfo.Mods.Count(x => x.ModType == nameof(Base)), Is.EqualTo(1));
        }

        [Test]
        public void Benny_DescribeFocusPoints_EnsureValues()
        {
            var pc = new PlayerCharacter(ActorFactory.BennyTemplate);
            var graph = new RpgGraph(pc);

            var propDesc = ObjectPropDescriber.Describe(graph, pc, "FocusPoints");
            Assert.That(propDesc, Is.Not.Null);

            Assert.That(propDesc.PropInfo.Value.ToString, Is.EqualTo("1"));
            Assert.That(propDesc.Archetype, Is.EqualTo("PlayerCharacter"));
            Assert.That(propDesc.PropPath, Is.EqualTo("FocusPoints"));

            Assert.That(propDesc.PropInfo.Mods.Count(), Is.EqualTo(5));
            Assert.That(propDesc.PropInfo.Mods.Count(x => x.ModType == nameof(Initial) || x.ModType == nameof(Threshold)), Is.EqualTo(2));
            Assert.That(propDesc.PropInfo.Mods.Count(x => x.ModType == nameof(Base)), Is.EqualTo(3));
        }
    }
}