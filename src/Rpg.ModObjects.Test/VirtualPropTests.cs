using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Templates;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Tests.Models;

namespace Rpg.ModObjects.Tests
{
    public class VirtualPropTests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeScan.RegisterAssembly(this.GetType().Assembly);
        }

        [Test] 
        public void AddVirtualProp_EnsureValues()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            entity.AddMod(new PermanentMod(), "VirtualProp", 1);
            graph.Time.TriggerEvent();

            Assert.That(graph.GetPropValue(entity, "VirtualProp")?.Roll(), Is.EqualTo(1));
        }
    }
}
