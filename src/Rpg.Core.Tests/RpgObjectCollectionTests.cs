using Rpg.Core.Tests.Models;
using Rpg.ModObjects;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Time;

namespace Rpg.Core.Tests
{
    public class RpgObjectCollectionTests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeScan.RegisterAssembly(this.GetType().Assembly);
            RpgTypeScan.RegisterAssembly(typeof(TestPerson).Assembly);
        }

        [Test]
        public void CreateDefault_EmptyContainer()
        {
            var person = new TestPerson("Benny");
            var graph = new RpgGraph(person);

            Assert.That(person.Hands, Is.Not.Null);
            Assert.That(person.Hands.Count(), Is.EqualTo(0));
        }

        [Test]
        public void CreateWithOneItem_SetBeforeCreate_ItemExists()
        {
            var person = new TestPerson("Benny");
            var weapon = new TestWeapon("Sword");
            person.Hands.Add(weapon);

            var graph = new RpgGraph(person);

            Assert.That(person.Hands.Count(), Is.EqualTo(1));
            Assert.That(person.Hands[0].Id, Is.EqualTo(weapon.Id));
        }

        [Test]
        public void CreateWithTwoItems_SetBeforeCreate_ItemExists()
        {
            var person = new TestPerson("Benny");
            var weapon = new TestWeapon("Sword");
            person.Hands.Add(weapon);

            var weapon2 = new TestWeapon("Sword2");
            person.Wearing.Add(weapon2);

            var graph = new RpgGraph(person);

            Assert.That(person.Hands.Count(), Is.EqualTo(1));
            Assert.That(person.Hands.Any(x => x.Id == weapon.Id), Is.True);
            Assert.That(person.Wearing.Count(), Is.EqualTo(1));
            Assert.That(person.Wearing.Any(x => x.Id == weapon2.Id), Is.True);
        }

        [Test]
        public void CreateWithTwoItems_SetBeforeCreate_RemoveItem2_Item1Exists()
        {
            var person = new TestPerson("Benny");
            var weapon = new TestWeapon("Sword");
            person.Hands.Add(weapon);

            var weapon2 = new TestWeapon("Sword2");
            person.Wearing.Add(weapon2);

            var graph = new RpgGraph(person);

            person.Wearing.Remove(weapon2);
            Assert.That(person.Hands.Count(), Is.EqualTo(1));
            Assert.That(person.Hands.Any(x => x.Id == weapon.Id), Is.True);
            Assert.That(person.Wearing.Count(), Is.EqualTo(0));
            Assert.That(person.Wearing.Any(x => x.Id == weapon2.Id), Is.False);
        }

        [Test]
        public void CreateWithTwoItems_SetBeforeCreate_RemoveItem2OnTurn2()
        {
            var person = new TestPerson("Benny");
            var weapon = new TestWeapon("Sword");
            person.Hands.Add(weapon);

            var weapon2 = new TestWeapon("Sword2");
            person.Wearing.Add(weapon2);

            var graph = new RpgGraph(person);

            graph.Time.Transition(2);
            person.Wearing.Remove(weapon2);

            Assert.That(person.Hands.Count(), Is.EqualTo(1));
            Assert.That(person.Hands.Any(x => x.Id == weapon.Id), Is.True);
            Assert.That(person.Wearing.Count(), Is.EqualTo(0));
            Assert.That(person.Wearing.Any(x => x.Id == weapon2.Id), Is.False);

            graph.Time.Transition(1);

            Assert.That(person.Hands.Count(), Is.EqualTo(1));
            Assert.That(person.Hands.Any(x => x.Id == weapon.Id), Is.True);
            Assert.That(person.Wearing.Count(), Is.EqualTo(1));
            Assert.That(person.Wearing.Any(x => x.Id == weapon2.Id), Is.True);
        }

        [Test]
        public void CreateWithTwoItems_SetBeforeCreate_RemoveItem2OnTurn2_EndEncounter()
        {
            var person = new TestPerson("Benny");
            var weapon = new TestWeapon("Sword");
            person.Hands.Add(weapon);

            var weapon2 = new TestWeapon("Sword2");
            person.Wearing.Add(weapon2);

            var graph = new RpgGraph(person);

            graph.Time.Transition(2);
            person.Wearing.Remove(weapon2);

            Assert.That(person.Hands.Count(), Is.EqualTo(1));
            Assert.That(person.Hands.Any(x => x.Id == weapon.Id), Is.True);
            Assert.That(person.Wearing.Count(), Is.EqualTo(0));
            Assert.That(person.Wearing.Any(x => x.Id == weapon2.Id), Is.False);

            graph.Time.Transition(PointInTimeType.EncounterEnds);

            Assert.That(person.Hands.Count(), Is.EqualTo(1));
            Assert.That(person.Hands.Any(x => x.Id == weapon.Id), Is.True);
            Assert.That(person.Wearing.Count(), Is.EqualTo(0));
            Assert.That(person.Wearing.Any(x => x.Id == weapon2.Id), Is.False);
        }
    }
}
