using Rpg.ModObjects.Meta;
using Rpg.ModObjects.Reflection;

namespace Rpg.Cyborgs.Tests
{
    public class MetaGraphTests
    {
        [SetUp]
        public void Setup()
        {
            RpgReflection.RegisterAssembly(typeof(CyborgsSystem).Assembly);
        }

        [Test]
        public void MetaGraph_Build_EnsureValues()
        {
            var meta = new MetaGraph();
            var system = meta.Build();

            Assert.That(system, Is.Not.Null);
        }
    }
}