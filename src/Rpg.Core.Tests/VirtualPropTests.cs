using Rpg.Core.Tests.Models;
using Rpg.ModObjects;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Mods;
using Rpg.ModObjects.Reflection;

namespace Rpg.Core.Tests
{
    public class VirtualPropTests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeScan.RegisterAssembly(this.GetType().Assembly);
            RpgTypeScan.RegisterAssembly(typeof(TestPerson).Assembly);
        }

        [Test] 
        public void AddVirtualProp_EnsureValues()
        {
            var person = new TestPerson("Benny");
            var graph = new RpgGraph(person);

            person.AddMod(new Permanent(), "VirtualProp", 1);
            graph.Time.TriggerEvent();

            Assert.That(graph.GetPropValue(person, "VirtualProp")?.Roll(), Is.EqualTo(1));
        }
    }
}
