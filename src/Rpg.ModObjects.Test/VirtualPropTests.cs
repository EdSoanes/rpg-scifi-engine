using Rpg.ModObjects.Modifiers;
using Rpg.ModObjects.Tests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Tests
{
    public class VirtualPropTests
    {
        [SetUp]
        public void Setup()
        {
            RpgGraphExtensions.RegisterAssembly(Assembly.GetExecutingAssembly());
        }

        [Test] 
        public void AddVirtualProp_EnsureValues()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            entity.AddMod(new Permanent(), "VirtualProp", 1);
            graph.TriggerUpdate();

            Assert.That(graph.GetPropValue(entity, "VirtualProp").Roll(), Is.EqualTo(1));
        }
    }
}
