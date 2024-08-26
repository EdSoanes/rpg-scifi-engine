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
        public void CreateEmptyRef_Unset()
        {
            var entity = new SimpleModdableEntity(10, 1);
            var graph = new RpgGraph(entity);

            Assert.That(entity.Ref.Get(), Is.Null);
            Assert.That(entity.Ref.Expiry, Is.EqualTo(LifecycleExpiry.Unset));
        }

        [Test] 
        public void CreatePermanentRef_Ok()
        {
            var entity = new SimpleModdableEntity(10, 1);
            var graph = new RpgGraph(entity);

            entity.Ref.Set("1234");

            Assert.That(entity.Ref.Get(), Is.EqualTo("1234"));

            graph.Time.Transition(PointInTimeType.TimePassing);
            Assert.That(entity.Ref.Get(), Is.EqualTo("1234"));

            graph.Time.Transition(PointInTimeType.Turn, 2);
            Assert.That(entity.Ref.Get(), Is.EqualTo("1234"));

            entity.Ref.Set("5678");

            Assert.That(entity.Ref.Get(), Is.EqualTo("5678"));

            graph.Time.Transition(PointInTimeType.Turn, 1);
            Assert.That(entity.Ref.Get(), Is.EqualTo("1234"));

            graph.Time.Transition(PointInTimeType.Turn, 1000);
            Assert.That(entity.Ref.Get(), Is.EqualTo("5678"));
        }
    }
}
