using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.ModObjects.Meta;

namespace Rpg.Sys.Tests
{
    public class MetaGraphTests
    {
        [SetUp]
        public void Setup()
        {
            MetaGraph.RegisterAssembly(typeof(MetaSystem).Assembly);
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
            var json = RpgSerializer.Serialize(system);

            Assert.That(json, Is.Not.Null);
        }

        [Test]
        public void CreateGraph_SerializeDeserialize_EnsureValues()
        {
            var metaGraph = new MetaGraph();
            var system = metaGraph.Build();
            var json = RpgSerializer.Serialize(system);

            var system2 = RpgSerializer.Deserialize<MetaSystem>(json)!;
            var json2 = RpgSerializer.Serialize(system2);

            Assert.That(json, Is.EqualTo(json2));
        }
    }
}