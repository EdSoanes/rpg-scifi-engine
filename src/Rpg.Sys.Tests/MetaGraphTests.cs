using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.ModObjects.Meta;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Server.Json;

namespace Rpg.Sys.Tests
{
    public class MetaGraphTests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeScan.RegisterAssembly(typeof(MetaSystem).Assembly);
        }

        [Test]
        public void CreateGraph_EnsureValues()
        {
            var metaGraph = new MetaGraph();
            var system = metaGraph.Build();

            Assert.That(system, Is.Not.Null);
        }

        [Test]
        public void CreateGraph_Serialize_EnsureValues()
        {
            var metaGraph = new MetaGraph();
            var system = metaGraph.Build();
            var json = RpgJson.Serialize(system);

            Assert.That(json, Is.Not.Null);
        }

        [Test]
        public void CreateGraph_SerializeDeserialize_EnsureValues()
        {
            var metaGraph = new MetaGraph();
            var system = metaGraph.Build();
            var json = RpgJson.Serialize(system);

            var system2 = RpgJson.Deserialize<MetaSystem>(json)!;
            var json2 = RpgJson.Serialize(system2);

            Assert.That(json, Is.EqualTo(json2));
        }
    }
}