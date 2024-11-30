using Rpg.Core.Tests.Models;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Time;

namespace Rpg.ModObjects.Tests
{
    public class RpgObject_ChildObjects
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeScan.RegisterAssembly(this.GetType().Assembly);
            RpgTypeScan.RegisterAssembly(typeof(TestPerson).Assembly);
        }

        [Test]
        public void CreateEmptyRef_Unset()
        {
            var person = new TestPerson("Benny");
            var graph = new RpgGraph(person);

            Assert.That(person.Enhancement, Is.Null);
        }

        [Test]
        public void CreateChild_SetBeforeCreate()
        {
            var person = new TestPerson("Benny");
            person.Enhancement = new TestEnhancement("Device");

            var graph = new RpgGraph(person);

            Assert.That(person.Enhancement, Is.Not.Null);
        }

        [Test]
        public void CreateChild_SetBeforeCreate_ReplaceOnTurn2()
        {
            var person = new TestPerson("Benny");
            person.Enhancement = new TestEnhancement("Device");

            var graph = new RpgGraph(person);

            graph.Time.Transition(2);

            var oldDevice = person.Enhancement;
            var newDevice = new TestEnhancement("Device2");
            person.Enhancement = newDevice;

            Assert.That(person.Enhancement, Is.Not.Null);
            Assert.That(person.Enhancement.Id, Is.EqualTo(newDevice.Id));

            graph.Time.Transition(1);
            Assert.That(person.Enhancement, Is.Not.Null);
            Assert.That(person.Enhancement.Id, Is.EqualTo(oldDevice.Id));
        }

        [Test]
        public void CreateChild_SetBeforeCreate_ReplaceOnTurn2_EndEncounter()
        {
            var person = new TestPerson("Benny");
            person.Enhancement = new TestEnhancement("Device");

            var graph = new RpgGraph(person);

            graph.Time.Transition(2);

            var oldDevice = person.Enhancement;
            var newDevice = new TestEnhancement("Device2");
            person.Enhancement = newDevice;

            Assert.That(person.Enhancement, Is.Not.Null);
            Assert.That(person.Enhancement.Id, Is.EqualTo(newDevice.Id));

            graph.Time.Transition(PointInTimeType.EncounterEnds);

            Assert.That(person.Enhancement, Is.Not.Null);
            Assert.That(person.Enhancement.Id, Is.EqualTo(newDevice.Id));
        }

        [Test]
        public void RemoveChild_SetBeforeCreate()
        {
            var person = new TestPerson("Benny");
            person.Enhancement = new TestEnhancement("Device");

            var graph = new RpgGraph(person);

            Assert.That(person.Enhancement, Is.Not.Null);
            Assert.That(person.Enhancement.GetParentObject(), Is.Not.Null);

            var oldChild = person.Enhancement;
            person.Enhancement = null;

            Assert.That(person.Enhancement, Is.Null);
            Assert.That(oldChild.GetParentObject(), Is.Null);
        }

        [Test]
        public void CreateChild_SetAfterCreate()
        {
            var person = new TestPerson("Benny");
            var graph = new RpgGraph(person);

            Assert.That(person.Enhancement, Is.Null);

            person.Enhancement = new TestEnhancement("Device2");
            Assert.That(person.Enhancement, Is.Not.Null);
        }
    }
}
