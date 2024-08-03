using Rpg.ModObjects.Meta;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.States;
using Rpg.Sys.Archetypes;

namespace Rpg.Sys.Tests
{
    public class MetaObjectTests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeScan.RegisterAssembly(typeof(MetaSystem).Assembly);
        }

        [Test]
        public void CreateActorTemplate_MetaObj_EnsureValues()
        {
            var gen = new MetaGraph();
            var obj = gen.Object(typeof(ActorTemplate), Array.Empty<MetaAction>(), Array.Empty<MetaState>());

            Assert.That(obj, Is.Not.Null);
        }

        [Test]
        public void CreateHuman_MetaObj_EnsureValues()
        {
            var gen = new MetaGraph();
            var obj = gen.Object(typeof(Human), Array.Empty<MetaAction>(), Array.Empty<MetaState>());

            Assert.That(obj, Is.Not.Null);
        }
    }
}