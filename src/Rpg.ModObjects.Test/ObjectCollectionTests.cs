using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Tests
{
    public class EntityWithCollection : RpgEntity
    {
        public int Prop { get; set; }

        public RpgObjectCollection Container { get; private set; }

        public EntityWithCollection(string name)
            : base(name) 
        {
            Container = new RpgObjectCollection(Id, nameof(Container));
        }

        public override void OnCreating(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnCreating(graph, entity);
            Container.OnCreating(graph, entity);
        }

        public override void OnTimeBegins()
        {
            base.OnTimeBegins();
            Container.OnTimeBegins();
        }

        public override void OnRestoring(RpgGraph graph, RpgObject? entity)
        {
            base.OnRestoring(graph, entity);
            Container.OnRestoring(graph, entity);
        }

        public override LifecycleExpiry OnStartLifecycle()
        {
            base.OnStartLifecycle();
            Container.OnStartLifecycle();
            return Expiry;
        }

        public override LifecycleExpiry OnUpdateLifecycle()
        {
            base.OnUpdateLifecycle();
            Container.OnUpdateLifecycle();
            return Expiry;
        }
    }

    public class ContainerEntity : RpgEntity
    {
        public int Prop { get; set; }
        public ContainerEntity(string name)
            : base(name) { }
    }

    public class ObjectCollectionTests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeScan.RegisterAssembly(this.GetType().Assembly);
        }

        [Test]
        public void CreateDefault_EmptyContainer()
        {
            var entity = new EntityWithCollection("Parent");
            var graph = new RpgGraph(entity);

            Assert.That(entity.Container, Is.Not.Null);
            Assert.That(entity.Container, Is.Not.Null);
            Assert.That(entity.Container.Count(), Is.EqualTo(0));
        }

        [Test]
        public void CreateWithOneItem_SetBeforeCreate_ItemExists()
        {
            var entity = new EntityWithCollection("Parent");
            var child1 = new ContainerEntity("Child1");
            entity.Container.Add(child1);

            var graph = new RpgGraph(entity);

            Assert.That(entity.Container.Count(), Is.EqualTo(1));
            Assert.That(entity.Container[0].Id, Is.EqualTo(child1.Id));
        }

        [Test]
        public void CreateWithTwoItems_SetBeforeCreate_ItemExists()
        {
            var entity = new EntityWithCollection("Parent");
            var child1 = new ContainerEntity("Child1");
            entity.Container.Add(child1);
            var child2 = new ContainerEntity("Child2");
            entity.Container.Add(child2);

            var graph = new RpgGraph(entity);

            Assert.That(entity.Container.Count(), Is.EqualTo(2));
            Assert.That(entity.Container.Any(x => x.Id == child1.Id), Is.True);
            Assert.That(entity.Container.Any(x => x.Id == child2.Id), Is.True);
        }

        [Test]
        public void CreateWithTwoItems_SetBeforeCreate_RemoveItem2_Item1Exists()
        {
            var entity = new EntityWithCollection("Parent");
            var child1 = new ContainerEntity("Child1");
            entity.Container.Add(child1);
            var child2 = new ContainerEntity("Child2");
            entity.Container.Add(child2);

            var graph = new RpgGraph(entity);

            entity.Container.Remove(child2);

            Assert.That(entity.Container.Count(), Is.EqualTo(1));
            Assert.That(entity.Container.Any(x => x.Id == child1.Id), Is.True);
            Assert.That(entity.Container.Any(x => x.Id == child2.Id), Is.False);
        }

        [Test]
        public void CreateWithTwoItems_SetBeforeCreate_RemoveItem2OnTurn2()
        {
            var entity = new EntityWithCollection("Parent");
            var child1 = new ContainerEntity("Child1");
            entity.Container.Add(child1);
            var child2 = new ContainerEntity("Child2");
            entity.Container.Add(child2);

            var graph = new RpgGraph(entity);
            graph.Time.Transition(2);
            entity.Container.Remove(child2);

            Assert.That(entity.Container.Count(), Is.EqualTo(1));
            Assert.That(entity.Container.Any(x => x.Id == child1.Id), Is.True);
            Assert.That(entity.Container.Any(x => x.Id == child2.Id), Is.False);

            graph.Time.Transition(1);

            Assert.That(entity.Container.Count(), Is.EqualTo(2));
            Assert.That(entity.Container.Any(x => x.Id == child1.Id), Is.True);
            Assert.That(entity.Container.Any(x => x.Id == child2.Id), Is.True);
        }

        [Test]
        public void CreateWithTwoItems_SetBeforeCreate_RemoveItem2OnTurn2_EndEncounter()
        {
            var entity = new EntityWithCollection("Parent");
            var child1 = new ContainerEntity("Child1");
            entity.Container.Add(child1);
            var child2 = new ContainerEntity("Child2");
            entity.Container.Add(child2);

            var graph = new RpgGraph(entity);
            graph.Time.Transition(2);
            entity.Container.Remove(child2);

            Assert.That(entity.Container.Count(), Is.EqualTo(1));
            Assert.That(entity.Container.Any(x => x.Id == child1.Id), Is.True);
            Assert.That(entity.Container.Any(x => x.Id == child2.Id), Is.False);

            graph.Time.Transition(Time.PointInTimeType.EncounterEnds);

            Assert.That(entity.Container.Count(), Is.EqualTo(1));
            Assert.That(entity.Container.Any(x => x.Id == child1.Id), Is.True);
            Assert.That(entity.Container.Any(x => x.Id == child2.Id), Is.False);
        }
    }
}
