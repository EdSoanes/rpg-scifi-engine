using Rpg.ModObjects.Reflection;

namespace Rpg.ModObjects.Tests
{
    public class ParentEntityWithContainer : RpgEntity
    {
        public int Prop { get; set; }

        public RpgContainer Container { get; private set; } = new RpgContainer();

        public ParentEntityWithContainer(string name)
            : base(name) { }
    }

    public class ContainerEntity : RpgEntity
    {
        public int Prop { get; set; }
        public ContainerEntity(string name)
            : base(name) { }
    }

    public class ObjectRefContainerTests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeScan.RegisterAssembly(this.GetType().Assembly);
        }

        [Test]
        public void CreateDefault_EmptyContainer()
        {
            var entity = new ParentEntityWithContainer("Parent");
            var graph = new RpgGraph(entity);

            Assert.That(entity.Container, Is.Not.Null);
            Assert.That(entity.Container.Contents, Is.Not.Null);
            Assert.That(entity.Container.Contents.Count(), Is.EqualTo(0));
        }

        [Test]
        public void CreateWithOneItem_SetBeforeCreate_ItemExists()
        {
            var entity = new ParentEntityWithContainer("Parent");
            var child1 = new ContainerEntity("Child1");
            entity.Container.Add(child1);

            var graph = new RpgGraph(entity);

            Assert.That(entity.Container.Contents.Count(), Is.EqualTo(1));
            Assert.That(entity.Container.Contents[0].Id, Is.EqualTo(child1.Id));
        }

        [Test]
        public void CreateWithTwoItems_SetBeforeCreate_ItemExists()
        {
            var entity = new ParentEntityWithContainer("Parent");
            var child1 = new ContainerEntity("Child1");
            entity.Container.Add(child1);
            var child2 = new ContainerEntity("Child2");
            entity.Container.Add(child2);

            var graph = new RpgGraph(entity);

            Assert.That(entity.Container.Contents.Count(), Is.EqualTo(2));
            Assert.That(entity.Container.Contents.Any(x => x.Id == child1.Id), Is.True);
            Assert.That(entity.Container.Contents.Any(x => x.Id == child2.Id), Is.True);
        }

        [Test]
        public void CreateWithTwoItems_SetBeforeCreate_RemoveItem2_Item1Exists()
        {
            var entity = new ParentEntityWithContainer("Parent");
            var child1 = new ContainerEntity("Child1");
            entity.Container.Add(child1);
            var child2 = new ContainerEntity("Child2");
            entity.Container.Add(child2);

            var graph = new RpgGraph(entity);

            entity.Container.Remove(child2);

            Assert.That(entity.Container.Contents.Count(), Is.EqualTo(1));
            Assert.That(entity.Container.Contents.Any(x => x.Id == child1.Id), Is.True);
            Assert.That(entity.Container.Contents.Any(x => x.Id == child2.Id), Is.False);
        }

        [Test]
        public void CreateWithTwoItems_SetBeforeCreate_RemoveItem2OnTurn2()
        {
            var entity = new ParentEntityWithContainer("Parent");
            var child1 = new ContainerEntity("Child1");
            entity.Container.Add(child1);
            var child2 = new ContainerEntity("Child2");
            entity.Container.Add(child2);

            var graph = new RpgGraph(entity);
            graph.Time.Transition(2);
            entity.Container.Remove(child2);

            Assert.That(entity.Container.Contents.Count(), Is.EqualTo(1));
            Assert.That(entity.Container.Contents.Any(x => x.Id == child1.Id), Is.True);
            Assert.That(entity.Container.Contents.Any(x => x.Id == child2.Id), Is.False);

            graph.Time.Transition(1);

            Assert.That(entity.Container.Contents.Count(), Is.EqualTo(2));
            Assert.That(entity.Container.Contents.Any(x => x.Id == child1.Id), Is.True);
            Assert.That(entity.Container.Contents.Any(x => x.Id == child2.Id), Is.True);
        }

        [Test]
        public void CreateWithTwoItems_SetBeforeCreate_RemoveItem2OnTurn2_EndEncounter()
        {
            var entity = new ParentEntityWithContainer("Parent");
            var child1 = new ContainerEntity("Child1");
            entity.Container.Add(child1);
            var child2 = new ContainerEntity("Child2");
            entity.Container.Add(child2);

            var graph = new RpgGraph(entity);
            graph.Time.Transition(2);
            entity.Container.Remove(child2);

            Assert.That(entity.Container.Contents.Count(), Is.EqualTo(1));
            Assert.That(entity.Container.Contents.Any(x => x.Id == child1.Id), Is.True);
            Assert.That(entity.Container.Contents.Any(x => x.Id == child2.Id), Is.False);

            graph.Time.Transition(Time.PointInTimeType.EncounterEnds);

            Assert.That(entity.Container.Contents.Count(), Is.EqualTo(1));
            Assert.That(entity.Container.Contents.Any(x => x.Id == child1.Id), Is.True);
            Assert.That(entity.Container.Contents.Any(x => x.Id == child2.Id), Is.False);
        }
    }
}
