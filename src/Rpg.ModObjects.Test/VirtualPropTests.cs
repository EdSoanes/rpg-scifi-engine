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
            ModGraphExtensions.RegisterAssembly(Assembly.GetExecutingAssembly());
        }

        [Test] 
        public void AddVirtualProp_EnsureValues()
        {
            var entity = new ModdableEntity();
            var graph = new ModGraph(entity);

            entity.AddPermanentMod("VirtualProp", 1);
            entity.TriggerUpdate();

            Assert.That(entity.GetPropValue("VirtualProp")?.Roll() ?? 0, Is.EqualTo(1));
        }
    }
}
