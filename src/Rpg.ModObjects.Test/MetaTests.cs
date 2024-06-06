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
            var objects = meta.GetObjects();

            Assert.That(objects, Is.Not.Empty);
        }

        [Test]
        public void MetaGraph_Serialize_EnsureValues()
        {
            var meta = new MetaGraph();
            var objects = meta.GetObjects();

            var json = RpgSerializer.Serialize(objects);

            Assert.That(json, Is.Not.Null);
        }
    }
}
