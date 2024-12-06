using Rpg.Core.Tests.Models;
using Rpg.ModObjects;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Mods;
using Rpg.ModObjects.Mods.ModSets;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Time;

namespace Rpg.Core.Tests
{
    public class RpgObject_ModSetTests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeScan.RegisterAssembly(this.GetType().Assembly);
            RpgTypeScan.RegisterAssembly(typeof(TestPerson).Assembly);
        }

        [Test]
        public void AddModSet_VerifyValues()
        {
            var person = new TestPerson("Benny");
            var graph = new RpgGraph(person);

            person.AddModSet(new ModSet(person.Id, "name")
                .Add(person, x => x.MeleeAttack, 1)
                .Add(person, x => x.HitPoints, 1));

            graph.Time.TriggerEvent();

            Assert.That(person.MeleeAttack, Is.EqualTo(3));
            Assert.That(person.HitPoints, Is.EqualTo(11));
        }

        [Test]
        public void AddModSet_ExpireModSet_VerifyValues()
        {
            var person = new TestPerson("Benny");
            var graph = new RpgGraph(person);

            person.AddModSet(new ModSet(person.Id, "name")
                .Add(person, x => x.MeleeAttack, 1)
                .Add(person, x => x.HitPoints, 1));

            graph.Time.TriggerEvent();

            var modSet = graph.GetModSets(person, (x) => x.Name == "name").First();
            modSet.SetExpired();
            graph.Time.TriggerEvent();

            Assert.That(person.MeleeAttack, Is.EqualTo(2));
            Assert.That(person.HitPoints, Is.EqualTo(10));
        }

        [Test]
        public void AddModSetWithPermanentMod_ExpireModSet_VerifyValues()
        {
            var person = new TestPerson("Benny");
            var graph = new RpgGraph(person);

            person.AddModSet(new ModSet(person.Id, "name")
                .Add(person, x => x.MeleeAttack, 1)
                .Add(new Permanent(), person, x => x.HitPoints, 1));

            graph.Time.TriggerEvent();

            var modSet = graph.GetModSets(person, (x) => x.Name == "name").First();
            modSet.SetExpired();
            graph.Time.TriggerEvent();

            Assert.That(person.MeleeAttack, Is.EqualTo(2));
            Assert.That(person.HitPoints, Is.EqualTo(11));
        }

        [Test]
        public void AddModSet_UnapplyAndApplyModSet_VerifyValues()
        {
            var person = new TestPerson("Benny");
            var graph = new RpgGraph(person);

            var modSet = new ModSet(person.Id, "name")
                .Add(person, x => x.MeleeAttack, 1)
                .Add(person, x => x.HitPoints, 1);

            person.AddModSet(modSet);

            graph.Time.TriggerEvent();

            Assert.That(person.MeleeAttack, Is.EqualTo(3));
            Assert.That(person.HitPoints, Is.EqualTo(11));

            modSet.Unapply();
            graph.Time.TriggerEvent();

            Assert.That(person.MeleeAttack, Is.EqualTo(2));
            Assert.That(person.HitPoints, Is.EqualTo(10));

            modSet.Apply();
            graph.Time.TriggerEvent();

            Assert.That(person.MeleeAttack, Is.EqualTo(3));
            Assert.That(person.HitPoints, Is.EqualTo(11));
        }

        [Test]
        public void AddModSet_DisableAndEnableModSet_VerifyValues()
        {
            var person = new TestPerson("Benny");
            var graph = new RpgGraph(person);

            var modSet = new ModSet(person.Id, "name")
                .Add(person, x => x.MeleeAttack, 1)
                .Add(person, x => x.HitPoints, 1);

            person.AddModSet(modSet);

            graph.Time.TriggerEvent();

            Assert.That(person.MeleeAttack, Is.EqualTo(3));
            Assert.That(person.HitPoints, Is.EqualTo(11));

            modSet.UserDisabled();
            graph.Time.TriggerEvent();

            Assert.That(person.MeleeAttack, Is.EqualTo(2));
            Assert.That(person.HitPoints, Is.EqualTo(10));

            modSet.UserEnabled();
            graph.Time.TriggerEvent();

            Assert.That(person.MeleeAttack, Is.EqualTo(3));
            Assert.That(person.HitPoints, Is.EqualTo(11));
        }

        [Test]
        public void AddModSet_RemoveModSet_VerifyValues()
        {
            var person = new TestPerson("Benny");
            var graph = new RpgGraph(person);

            var modSet = new ModSet(person.Id, "name")
                .Add(person, x => x.MeleeAttack, 1)
                .Add(person, x => x.HitPoints, 1);

            person.AddModSet(modSet);

            graph.Time.TriggerEvent();

            Assert.That(person.ModSets.Count(), Is.EqualTo(1));
            Assert.That(person.MeleeAttack, Is.EqualTo(3));
            Assert.That(person.HitPoints, Is.EqualTo(11));

            graph.RemoveModSet(person, modSet.Id);
            graph.Time.TriggerEvent();

            Assert.That(person.ModSets.Count(), Is.EqualTo(0));
            Assert.That(person.MeleeAttack, Is.EqualTo(2));
            Assert.That(person.HitPoints, Is.EqualTo(10));
        }

        [Test]
        public void AddEncounterModSet_EndEncounter_VerifyValues()
        {
            var person = new TestPerson("Benny");
            var graph = new RpgGraph(person);

            graph.Time.Transition(PointInTimeType.Turn);

            var modSet = new TimedModSet("name", new Lifespan(PointInTimeType.EncounterBegins, PointInTimeType.EncounterEnds))
                .Add(person, x => x.MeleeAttack, 1)
                .Add(person, x => x.HitPoints, 1);

            person.AddModSet(modSet);

            graph.Time.TriggerEvent();

            Assert.That(person.ModSets.Count(), Is.EqualTo(1));
            Assert.That(person.MeleeAttack, Is.EqualTo(3));
            Assert.That(person.HitPoints, Is.EqualTo(11));

            graph.Time.Transition(PointInTimeType.EncounterEnds);

            Assert.That(person.ModSets.Count(), Is.EqualTo(0));
            Assert.That(person.MeleeAttack, Is.EqualTo(2));
            Assert.That(person.HitPoints, Is.EqualTo(10));
        }
    }
}
