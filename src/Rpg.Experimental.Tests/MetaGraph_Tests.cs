using Rpg.Experimental.Meta;
using Rpg.Experimental.Reflection;
using Rpg.Experimental.Tests.Models;

namespace Rpg.Experimental.Tests
{
    public class MetaGraph_Tests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeUtilities.RegisterAssembly(typeof(TestSystem).Assembly);
        }

        [Test]
        public void LoadGraph_EnsureValues()
        {
            var meta = new MetaGraph();
            var system = meta.Build();

            Assert.That(system, Is.Not.Null);
        }
    }
}