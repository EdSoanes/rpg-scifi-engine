using Rpg.ModObjects.Meta;
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

        [Test]
        public void MetaGraph_Serialize_EnsureValues()
        {
            var meta = new MetaGraph();
            var system = meta.Build();

            var json = RpgSerializer.Serialize(system);

            Assert.That(json, Is.Not.Null);
        }
    }
}
