using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Mods;
using Rpg.ModObjects.Refs;

namespace Rpg.ModObjects.Tests
{
    public class ScopedComponent : RpgComponent
    {
        public int Prop {  get; set; }

        public ScopedComponent(string name) 
            : base(name) { }
    }

    public class ScopedEntity : RpgEntity
    {
        public int Prop { get; set; }

        public ScopedComponent ChildComponent1 { get; set; } = new ScopedComponent("ChildComponent1");

        private RpgObjectRef<ScopedComponent> _childComponent2 = new RpgObjectRef<ScopedComponent>(RelationshipType.Child);
        public ScopedComponent? ChildComponent2
        {
            get => _childComponent2;
            set => _childComponent2 = SetAsChild(value, _childComponent2);
        }

        private ChildEntity? _childEntity;
        public ChildEntity? ChildEntity
        {
            get => _childEntity;
            set => _childEntity = SetAsChild(value, _childEntity);
        }

        public ScopedEntity()
        {
            Name = "ScopedEntity";
            ChildEntity = new ChildEntity("ChildEntity");
        }
    }

    public class ChildEntity : RpgEntity
    {
        public int Prop { get; set; }

        public ChildEntity(string name)
            : base(name) { }
    }

    public class ScopedEntityModTests
    {
        [Test]
        public void ScopedEntity_AddComponentMod_VerifyProps()
        {
            var entity = new ScopedEntity();
            entity.ChildComponent2 = new ScopedComponent("ChildComponent2");

            var graph = new RpgGraph(entity);

            Assert.That(entity.Prop, Is.EqualTo(0));
            Assert.That(entity.ChildComponent1.Prop, Is.EqualTo(0));
            Assert.That(entity.ChildComponent2.Prop, Is.EqualTo(0));
            Assert.That(entity.ChildEntity!.Prop, Is.EqualTo(0));

            entity.AddMod(new Permanent(ModScope.ChildComponents), "Prop", 1);
            graph.Time.TriggerEvent();

            Assert.That(entity.Prop, Is.EqualTo(0));
            Assert.That(entity.ChildComponent1.Prop, Is.EqualTo(1));
            Assert.That(entity.ChildComponent2.Prop, Is.EqualTo(1));
            Assert.That(entity.ChildEntity!.Prop, Is.EqualTo(0));
        }

        [Test]
        public void ScopedEntity_GetChildComponents()
        {
            var entity = new ScopedEntity();
            var graph = new RpgGraph(entity);

            var components = graph.GetObjectsByScope(entity.Id, ModScope.ChildComponents).ToArray();
            
            Assert.That(components.Count(), Is.EqualTo(1));
            Assert.That(components[0], Is.AssignableTo(typeof(ScopedComponent)));
        }

        [Test]
        public void ScopedEntity_SetOptionalChildComponent_GetChildComponents()
        {
            var entity = new ScopedEntity();
            var graph = new RpgGraph(entity);

            entity.ChildComponent2 = new ScopedComponent("ChildComponent2");

            var components = graph.GetObjectsByScope(entity.Id, ModScope.ChildComponents).ToArray();

            Assert.That(components.Count(), Is.EqualTo(2));
            Assert.That(components[0], Is.AssignableTo(typeof(ScopedComponent)));
            Assert.That(components[1], Is.AssignableTo(typeof(ScopedComponent)));
        }


        [Test]
        public void ScopedEntity_GetChildObjects()
        {
            var entity = new ScopedEntity();
            var graph = new RpgGraph(entity);

            var components = graph.GetObjectsByScope(entity.Id, ModScope.ChildObjects).ToArray();

            Assert.That(components.Count(), Is.EqualTo(2));

            var c1 = components.FirstOrDefault(x => x.Name == "ChildComponent1");
            Assert.That(c1, Is.Not.Null);
            Assert.That(c1, Is.AssignableTo<ScopedComponent>());

            var e1 = components.FirstOrDefault(x => x.Name == "ChildEntity");
            Assert.That(e1, Is.Not.Null);
            Assert.That(e1, Is.AssignableTo<ChildEntity>());
        }

        [Test]
        public void ScopedEntity_GetParentObject()
        {
            var entity = new ScopedEntity();
            var graph = new RpgGraph(entity);

            var parent = graph.GetObjectsByScope(entity.ChildComponent1.Id, ModScope.ParentEntity).ToArray();

            Assert.That(parent.Count(), Is.EqualTo(1));

            var p1 = parent.FirstOrDefault(x => x.Name == "ScopedEntity");
            Assert.That(p1, Is.Not.Null);
            Assert.That(p1, Is.AssignableTo<ScopedEntity>());
        }
    }
}
