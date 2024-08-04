using Rpg.ModObjects.Meta;
using Rpg.ModObjects.Reflection;

namespace Rpg.Cyborgs.Tests
{
    public class MetaGraphTests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeScan.RegisterAssembly(typeof(CyborgsSystem).Assembly);
        }

        [Test]
        public void TakeDamageGroup_CreateActivity_EnsureInstances()
        {
            var meta = new MetaGraph();
            var system = meta.Build();

            Assert.That(system, Is.Not.Null);
        }
    }
}