using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Templates;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Tests.Models;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Tests
{
    public class RpgRefTests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeScan.RegisterAssembly(this.GetType().Assembly);
        }

        [Test] 
        public void CreatePermanentRef_Ok()
        {
            var graph = new RpgGraph(new SimpleModdableEntity(10, 1));
            var objRef = new RpgRef(graph, "1234");

            Assert.That(objRef.EntityId, Is.EqualTo("1234"));

            graph.Time.Transition(PointInTimeType.TimePassing);
            Assert.That(objRef.EntityId, Is.EqualTo("1234"));

            graph.Time.Transition(PointInTimeType.Turn, 2);
            Assert.That(objRef.EntityId, Is.EqualTo("1234"));

            objRef.EntityId = "5678";
            Assert.That(objRef.EntityId, Is.EqualTo("5678"));

            graph.Time.Transition(PointInTimeType.Turn, 1);
            Assert.That(objRef.EntityId, Is.EqualTo("1234"));

            graph.Time.Transition(PointInTimeType.Turn, 1000);
            Assert.That(objRef.EntityId, Is.EqualTo("5678"));

        }
    }
}
