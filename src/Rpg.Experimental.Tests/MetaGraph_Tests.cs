using Rpg.Experimental.Graph;
using Rpg.Experimental.Reflection;
using Rpg.Experimental.System;
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
            var meta = RpgSystemFactory.Build();

            Assert.That(meta, Is.Not.Null);
        }

        [Test]
        public void GetObjectProperties_EnsureValues()
        {
            var obj = new TestObject();
            obj.Child = new TestObject();

            var graph = new RpgGraph(obj);
            graph.Time.Refresh();

            var properties = graph.GetProperties(obj.Id);
            Assert.That(properties.Any(), Is.True);
        }
    }
}