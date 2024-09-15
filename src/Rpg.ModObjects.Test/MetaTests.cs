using Rpg.ModObjects.Meta;
using Rpg.ModObjects.Server.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Tests
{
    public class MetaTests
    {
        [Test]
        public void MetaGraph_EnsureValues()
        {
            var meta = new MetaGraph();
            var system = meta.Build();

            Assert.That(system, Is.Not.Null);
        }
    }
}
