using Rpg.Core.Tests.Models;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Server.Json;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Server.Tests
{
    public class ParentClass : RpgEntity
    {
        public RpgObjectCollection Objects { get; set; }

        public ParentClass()
        {
            Objects = new RpgObjectCollection(Id, nameof(Objects));
        }

        public override void OnCreating(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnCreating(graph, entity);
            Objects.OnCreating(graph, this);
        }

        public override void OnRestoring(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnRestoring(graph, entity);
            Objects.OnRestoring(graph, this);
        }

        public override void OnTimeBegins()
        {
            base.OnTimeBegins();
            Objects.OnTimeBegins();
        }

        public override LifecycleExpiry OnStartLifecycle()
        {
            base.OnStartLifecycle();
            Objects.OnStartLifecycle();
            return Expiry;
        }

        public override LifecycleExpiry OnUpdateLifecycle()
        {
            base.OnUpdateLifecycle();
            Objects.OnUpdateLifecycle();
            return Expiry;
        }
    }

    public class ChildClass : RpgObject 
    { 
    }

    public class RpgObjectCollectionSerializationTests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeScan.RegisterAssembly(this.GetType().Assembly);
            RpgTypeScan.RegisterAssembly(typeof(TestPerson).Assembly);
        }

        [Test]
        public void TestEntity_Serialize_EnsureValues()
        {
            var entity = new ParentClass();
            entity.Objects.Add(new ChildClass());

            var graph = new RpgGraph(entity);

            var json = RpgJson.Serialize(graph.GetGraphState());

            var graphState = RpgJson.Deserialize<RpgGraphState>(json)!;
            var graph2 = new RpgGraph(graphState);

            var entity2 = graph2.Context as ParentClass;
            var children = entity2.Objects.ToList();

            Assert.That(entity2, Is.Not.Null);
            Assert.That(children.Count, Is.EqualTo(1));
        }

        [Test]
        public void RpgObjectCollection_Serialization()
        {
            var objs = new RpgObjectCollection();
            objs.Add(new ModdableEntity());
            var json = RpgJson.Serialize(objs);

            var objs2 = RpgJson.Deserialize<RpgObjectCollection>(json);
            Assert.That(objs2, Is.Not.Null);


        }
    }
}
