using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.ModObjects.Meta;
using Rpg.Sys.Archetypes;

namespace Rpg.Sys.Tests
{
    public class MetaGenTests
    {
        [SetUp]
        public void Setup()
        {
            MetaGraph.RegisterAssembly(typeof(MetaSystem).Assembly);
        }

        [Test]
        public void CreateActorTemplate_MetaObj_EnsureValues()
        {
            var gen = new MetaGen();
            var obj = gen.Object(typeof(ActorTemplate));

            Assert.That(obj, Is.Not.Null);
        }
    }
}