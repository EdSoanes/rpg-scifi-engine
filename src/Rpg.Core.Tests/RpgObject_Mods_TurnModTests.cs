using Rpg.Core.Tests.Models;
using Rpg.ModObjects;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Mods;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Time;

namespace Rpg.Core.Tests
{
    public class RpgObject_Mods_TurnModTests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeScan.RegisterAssembly(this.GetType().Assembly);
            RpgTypeScan.RegisterAssembly(typeof(TestPerson).Assembly);
        }

        [Test]
        public void AddTurnMod_Ensure_MeleeAttack()
        {
            var person = new TestPerson("Benny");
            var graph = new RpgGraph(person);

            Assert.That(person.MeleeAttack, Is.EqualTo(2));
        }

        [Test]
        public void AddTurnMod_IncrementTurn_TurnModRemoved()
        {
            var person = new TestPerson("Benny");
            var graph = new RpgGraph(person);

            graph.Time.Transition(PointInTimeType.EncounterBegins);

            person.AddMod(new Turn(), x => x.MeleeAttack, 2);
            graph.Time.TriggerEvent();

            Assert.That(person.MeleeAttack, Is.EqualTo(4));

            graph.Time.Transition(PointInTimeType.Turn, 2);

            Assert.That(person.MeleeAttack, Is.EqualTo(2));
        }

        [Test]
        public void AddTurnMod_RepeatSameTurn_TurnModNotRemoved()
        {
            var person = new TestPerson("Benny");
            var graph = new RpgGraph(person);

            graph.Time.Transition(PointInTimeType.Turn);

            person.AddMod(new Turn(), x => x.MeleeAttack, 2);
            graph.Time.TriggerEvent();
            Assert.That(person.MeleeAttack, Is.EqualTo(4));

            graph.Time.Transition(PointInTimeType.Turn, 1);
            Assert.That(person.MeleeAttack, Is.EqualTo(4));
        }

        [Test]
        public void AddTurnMod_RevertTurn_TurnModReapplied()
        {
            var person = new TestPerson("Benny");
            var graph = new RpgGraph(person);

            graph.Time.Transition(PointInTimeType.Turn);

            person.AddMod(new Turn(), x => x.MeleeAttack, 2);
            graph.Time.TriggerEvent();
            Assert.That(person.MeleeAttack, Is.EqualTo(4));

            graph.Time.Transition(PointInTimeType.Turn, 2);
            Assert.That(person.MeleeAttack, Is.EqualTo(2));

            graph.Time.Transition(PointInTimeType.Turn, 1);
            Assert.That(person.MeleeAttack, Is.EqualTo(4));
        }

        [Test]
        public void AddTurnMod_NewEncounter_TurnModRemoved()
        {
            var person = new TestPerson("Benny");
            var graph = new RpgGraph(person);

            var modCount = graph.GetActiveMods().Count();

            Assert.That(person.MeleeAttack, Is.EqualTo(2));
            graph.Time.Transition(PointInTimeType.EncounterBegins);

            person.AddMod(new Turn(), x => x.MeleeAttack, 2);
            graph.Time.TriggerEvent();
            Assert.That(person.MeleeAttack, Is.EqualTo(4));

            graph.Time.Transition(PointInTimeType.EncounterEnds);
            graph.Time.Transition(PointInTimeType.EncounterBegins);
            Assert.That(person.MeleeAttack, Is.EqualTo(2));
            Assert.That(graph.GetActiveMods().Count(), Is.EqualTo(modCount));
        }

        [Test]
        public void AddTurnMod_EndEncounter_TurnModRemoved()
        {
            var person = new TestPerson("Benny");
            var graph = new RpgGraph(person);

            var modCount = graph.GetActiveMods().Count();

            graph.Time.Transition(PointInTimeType.EncounterBegins);

            person.AddMod(new Turn(), x => x.MeleeAttack, 2);
            graph.Time.TriggerEvent();

            Assert.That(person.MeleeAttack, Is.EqualTo(4));

            graph.Time.Transition(PointInTimeType.EncounterEnds);

            Assert.That(person.MeleeAttack, Is.EqualTo(2));
            Assert.That(graph.GetActiveMods().Count(), Is.EqualTo(modCount));
        }

        [Test]
        public void AddTimedMod_NextTurn_TimedModRemovedAfter3Turns()
        {
            var person = new TestPerson("Benny");
            var graph = new RpgGraph(person);

            graph.Time.Transition(PointInTimeType.EncounterBegins);

            person.AddMod(new Turn(3), x => x.MeleeAttack, 2);
            graph.Time.TriggerEvent();

            Assert.That(person.MeleeAttack, Is.EqualTo(4));

            //Turn 2
            graph.Time.Transition(PointInTimeType.Turn, 2);
            Assert.That(person.MeleeAttack, Is.EqualTo(4));

            //Turn 3
            graph.Time.Transition(PointInTimeType.Turn, 3);
            Assert.That(person.MeleeAttack, Is.EqualTo(4));

            //Turn 4
            graph.Time.Transition(PointInTimeType.Turn, 4);
            Assert.That(person.MeleeAttack, Is.EqualTo(2));
        }

        [Test]
        public void AddTimedMod_RewindTurn_TimedModValid()
        {
            var person = new TestPerson("Benny");
            var graph = new RpgGraph(person);

            graph.Time.Transition(PointInTimeType.EncounterBegins);

            person.AddMod(new Turn(3), x => x.MeleeAttack, 2);
            graph.Time.TriggerEvent();
            Assert.That(person.MeleeAttack, Is.EqualTo(4));

            //Set to Turn 4
            graph.Time.Transition(PointInTimeType.Turn, 4);
            Assert.That(person.MeleeAttack, Is.EqualTo(2));

            //Set to Turn 3
            graph.Time.Transition(PointInTimeType.Turn, 3);
            Assert.That(person.MeleeAttack, Is.EqualTo(4));
        }
    }
}
