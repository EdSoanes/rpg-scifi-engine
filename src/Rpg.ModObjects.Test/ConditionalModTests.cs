using Rpg.ModObjects.Modifiers;
using Rpg.ModObjects.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Tests
{
    public class EntityComponent1 : RpgEntityComponent
    {
        public int Prop {  get; set; }
        public EntityComponent1(string entityId, string name) : base(entityId, name) { }
    }

    public class Entity1 : RpgEntity
    {
        public int Prop { get; set; }

        public EntityComponent1 Component1 { get; set; }
        public EntityComponent1 Component2 { get; set; }
        public Entity2 Entity2 { get; set; }

        public Entity1()
        {
            Component1 = new EntityComponent1(Id, "Component1");
            Component2 = new EntityComponent1(Id, "Component2");
            Entity2 = new Entity2();
        }
    }

    public class Entity2 : RpgEntity
    {
        public int Prop { get; set; }
    }

    public class ConditionalModTests
    {
        [Test]
        public void AddEntityConditionalMod_VerifyValues()
        {
            var entity = new Entity1();
            var graph = new RpgGraph(entity);

            Assert.That(entity.Prop, Is.EqualTo(0));
            Assert.That(entity.Component1.Prop, Is.EqualTo(0));
            Assert.That(entity.Component2.Prop, Is.EqualTo(0));
            Assert.That(entity.Entity2.Prop, Is.EqualTo(0));

            entity.AddMod(new PermanentMod().SetScope(ModScope.Components), x => x.Component2.Prop, 1);
            graph.Time.TriggerEvent();

            Assert.That(entity.Prop, Is.EqualTo(0));
            Assert.That(entity.Component1.Prop, Is.EqualTo(1));
            Assert.That(entity.Component2.Prop, Is.EqualTo(1));
            Assert.That(entity.Entity2.Prop, Is.EqualTo(0));

        }
    }
}
