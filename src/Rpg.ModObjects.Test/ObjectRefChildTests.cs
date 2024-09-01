using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Tests
{
    public class ParentRefEntity : RpgEntity
    {
        public int Prop { get; set; }

        public ChildRefEntity? Child
        {
            get => GetChildObject<ChildRefEntity>(nameof(Child));
            set => SetChildObject(nameof(Child), value);
        }

        public ParentRefEntity(string name)
            : base(name) { }
    }

    public class ChildRefEntity : RpgEntity
    {
        public int Prop { get; set; }
        public ChildRefEntity(string name)
            : base(name) { }
    }


    public class ObjectRefChildTests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeScan.RegisterAssembly(this.GetType().Assembly);
        }

        [Test]
        public void CreateEmptyRef_Unset()
        {
            var entity = new ParentRefEntity("Parent");
            var graph = new RpgGraph(entity);

            Assert.That(entity.Child, Is.Null);
        }

        [Test]
        public void CreateChild_SetBeforeCreate()
        {
            var entity = new ParentRefEntity("Parent");
            entity.Child = new ChildRefEntity("Child");
            var graph = new RpgGraph(entity);

            Assert.That(entity.Child, Is.Not.Null);
        }

        [Test]
        public void CreateChild_SetBeforeCreate_ReplaceOnTurn2()
        {
            var entity = new ParentRefEntity("Parent");
            entity.Child = new ChildRefEntity("Child");
            var graph = new RpgGraph(entity);

            Assert.That(entity.Child, Is.Not.Null);

            graph.Time.Transition(2);

            var oldChild = entity.Child;
            var newChild = new ChildRefEntity("Child2");
            entity.Child = newChild;

            Assert.That(entity.Child, Is.Not.Null);
            Assert.That(entity.Child.Id, Is.EqualTo(newChild.Id));

            graph.Time.Transition(1);
            Assert.That(entity.Child, Is.Not.Null);
            Assert.That(entity.Child.Id, Is.EqualTo(oldChild.Id));
        }

        [Test]
        public void CreateChild_SetBeforeCreate_ReplaceOnTurn2_EndEncounter()
        {
            var entity = new ParentRefEntity("Parent");
            entity.Child = new ChildRefEntity("Child");
            var graph = new RpgGraph(entity);

            Assert.That(entity.Child, Is.Not.Null);

            graph.Time.Transition(2);

            var oldChild = entity.Child;
            var newChild = new ChildRefEntity("Child2");
            entity.Child = newChild;

            Assert.That(entity.Child, Is.Not.Null);
            Assert.That(entity.Child.Id, Is.EqualTo(newChild.Id));

            graph.Time.Transition(PointInTimeType.EncounterEnds);

            Assert.That(entity.Child, Is.Not.Null);
            Assert.That(entity.Child.Id, Is.EqualTo(newChild.Id));
        }

        [Test]
        public void RemoveChild_SetBeforeCreate()
        {
            var entity = new ParentRefEntity("Parent");
            entity.Child = new ChildRefEntity("Child");
            var graph = new RpgGraph(entity);

            Assert.That(entity.Child, Is.Not.Null);
            Assert.That(entity.Child.GetParentObject(), Is.Not.Null);

            var oldChild = entity.Child;
            entity.Child = null;

            Assert.That(entity.Child, Is.Null);
            Assert.That(oldChild.GetParentObject(), Is.Null);
        }

        [Test]
        public void CreateChild_SetAfterCreate()
        {
            var entity = new ParentRefEntity("Parent");
            var graph = new RpgGraph(entity);

            Assert.That(entity.Child, Is.Null);

            entity.Child = new ChildRefEntity("Child");
            Assert.That(entity.Child, Is.Not.Null);
        }
    }
}
