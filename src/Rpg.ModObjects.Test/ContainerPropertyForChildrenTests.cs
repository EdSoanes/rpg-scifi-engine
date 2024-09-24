using Rpg.ModObjects.Reflection;

namespace Rpg.ModObjects.Tests
{
    public class ParentEntityWithChildren : RpgEntity
    {
        public int Prop { get; set; }

        public IEnumerable<RpgObject> Children { get => GetChildObjects(nameof(Children)); }

        public ParentEntityWithChildren(string name)
            : base(name) { }
    }

    public class ChildrenEntity : RpgEntity
    {
        public int Prop { get; set; }
        public ChildrenEntity(string name)
            : base(name) { }
    }

    public class ContainerPropertyForChildrenTests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeScan.RegisterAssembly(this.GetType().Assembly);
        }

        [Test]
        public void CreateDefault_EmptyContainer()
        {
            var entity = new ParentEntityWithChildren("Parent");
            var graph = new RpgGraph(entity);

            Assert.That(entity.Children, Is.Not.Null);
            Assert.That(entity.Children, Is.Not.Null);
            Assert.That(entity.Children.Count(), Is.EqualTo(0));
        }

        [Test]
        public void CreateWithOneItem_SetBeforeCreate_ItemExists()
        {
            var entity = new ParentEntityWithChildren("Parent");
            var child1 = new ContainerEntity("Child1");
            entity.AddChildren("Children", child1);

            var graph = new RpgGraph(entity);

            Assert.That(entity.Children.Count(), Is.EqualTo(1));
            Assert.That(entity.Children.First().Id, Is.EqualTo(child1.Id));
        }

        [Test]
        public void CreateWithTwoItems_SetBeforeCreate_ItemExists()
        {
            var entity = new ParentEntityWithChildren("Parent");
            var child1 = new ContainerEntity("Child1");
            entity.AddChildren("Children", child1);
            var child2 = new ContainerEntity("Child2");
            entity.AddChildren("Children", child2);

            var graph = new RpgGraph(entity);

            Assert.That(entity.Children.Count(), Is.EqualTo(2));
            Assert.That(entity.Children.FirstOrDefault(x => x.Id == child1.Id), Is.Not.Null);
            Assert.That(entity.Children.FirstOrDefault(x => x.Id == child2.Id), Is.Not.Null);
        }

        [Test]
        public void CreateWithTwoItems_SetBeforeCreate_RemoveItem2_Item1Exists()
        {
            var entity = new ParentEntityWithChildren("Parent");
            var child1 = new ContainerEntity("Child1");
            entity.AddChildren("Children", child1);
            var child2 = new ContainerEntity("Child2");
            entity.AddChildren("Children", child2);

            var graph = new RpgGraph(entity);

            entity.RemoveChildren("Children", child2);

            Assert.That(entity.Children.Count(), Is.EqualTo(1));
            Assert.That(entity.Children.FirstOrDefault(x => x.Id == child1.Id), Is.Not.Null);
            Assert.That(entity.Children.FirstOrDefault(x => x.Id == child2.Id), Is.Null);
        }

        [Test]
        public void CreateWithTwoItems_SetBeforeCreate_RemoveItem2OnTurn2()
        {
            var entity = new ParentEntityWithChildren("Parent");
            var child1 = new ContainerEntity("Child1");
            entity.AddChildren("Children", child1);
            var child2 = new ContainerEntity("Child2");
            entity.AddChildren("Children", child2);

            var graph = new RpgGraph(entity);
            graph.Time.Transition(2);
            entity.RemoveChildren("Children", child2);

            Assert.That(entity.Children.Count(), Is.EqualTo(1));
            Assert.That(entity.Children.FirstOrDefault(x => x.Id == child1.Id), Is.Not.Null);
            Assert.That(entity.Children.FirstOrDefault(x => x.Id == child2.Id), Is.Null);

            graph.Time.Transition(1);

            Assert.That(entity.Children.Count(), Is.EqualTo(2));
            Assert.That(entity.Children.FirstOrDefault(x => x.Id == child1.Id), Is.Not.Null);
            Assert.That(entity.Children.FirstOrDefault(x => x.Id == child2.Id), Is.Not.Null);
        }

        [Test]
        public void CreateWithTwoItems_SetBeforeCreate_RemoveItem2OnTurn2_EndEncounter()
        {
            var entity = new ParentEntityWithChildren("Parent");
            var child1 = new ContainerEntity("Child1");
            entity.AddChildren("Children", child1);
            var child2 = new ContainerEntity("Child2");
            entity.AddChildren("Children", child2);

            var graph = new RpgGraph(entity);
            graph.Time.Transition(2);
            entity.RemoveChildren("Children", child2);

            Assert.That(entity.Children.Count(), Is.EqualTo(1));
            Assert.That(entity.Children.FirstOrDefault(x => x.Id == child1.Id), Is.Not.Null);
            Assert.That(entity.Children.FirstOrDefault(x => x.Id == child2.Id), Is.Null);

            graph.Time.Transition(Time.PointInTimeType.EncounterEnds);

            Assert.That(entity.Children.Count(), Is.EqualTo(1));
            Assert.That(entity.Children.FirstOrDefault(x => x.Id == child1.Id), Is.Not.Null);
            Assert.That(entity.Children.FirstOrDefault(x => x.Id == child2.Id), Is.Null);
        }
    }
}
