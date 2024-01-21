using Rpg.Sys.Modifiers;
using System.Reflection;

namespace Rpg.Sys.Tests
{

    public class EncounterTests
    {
        [SetUp]
        public void Setup()
        {
            GraphExtensions.RegisterAssembly(Assembly.GetExecutingAssembly());
        }

        [Test]
        public void AddTurnMod_IncrementTurn_TurnModRemoved()
        {
            var entity = new TestEntity();
            var graph = new Graph();

            graph.SetContext(entity);

            Assert.That(entity.MeleeAttack, Is.EqualTo(10));

            graph.NewEncounter();

            graph.Add.Mods(TurnModifier.Create(entity, 2, x => x.MeleeAttack));
            Assert.That(entity.MeleeAttack, Is.EqualTo(12));

            graph.NewTurn();

            Assert.That(entity.MeleeAttack, Is.EqualTo(10));
        }

        [Test]
        public void AddTurnMod_RepeatSameTurn_TurnModNotRemoved()
        {
            var entity = new TestEntity();
            var graph = new Graph();

            graph.SetContext(entity);

            Assert.That(entity.MeleeAttack, Is.EqualTo(10));

            graph.NewEncounter();

            Assert.That(graph.Turn, Is.EqualTo(1));
            graph.Add.Mods(TurnModifier.Create(entity, 2, x => x.MeleeAttack));
            Assert.That(entity.MeleeAttack, Is.EqualTo(12));

            graph.SetTurn(1);
            Assert.That(graph.Turn, Is.EqualTo(1));
            Assert.That(entity.MeleeAttack, Is.EqualTo(12));
        }

        [Test]
        public void AddTurnMod_RevertTurn_TurnModReapplied()
        {
            var entity = new TestEntity();
            var graph = new Graph();

            graph.SetContext(entity);

            Assert.That(entity.MeleeAttack, Is.EqualTo(10));

            graph.NewEncounter();

            graph.Add.Mods(TurnModifier.Create(entity, 2, x => x.MeleeAttack));
            Assert.That(entity.MeleeAttack, Is.EqualTo(12));

            graph.NewTurn();
            Assert.That(entity.MeleeAttack, Is.EqualTo(10));

            graph.PrevTurn();
            Assert.That(entity.MeleeAttack, Is.EqualTo(12));
        }

        [Test]
        public void AddTurnMod_NewEncounter_TurnModRemoved()
        {
            var entity = new TestEntity();
            var graph = new Graph();

            graph.SetContext(entity);

            Assert.That(entity.MeleeAttack, Is.EqualTo(10));
            graph.Add.Mods(TurnModifier.Create(entity, 2, x => x.MeleeAttack));
            Assert.That(entity.MeleeAttack, Is.EqualTo(12));

            graph.NewEncounter();

            Assert.That(entity.MeleeAttack, Is.EqualTo(10));
        }

        [Test]
        public void AddTurnMod_EndEncounter_TurnModRemoved()
        {
            var entity = new TestEntity();
            var graph = new Graph();

            graph.SetContext(entity);
            graph.NewEncounter();

            Assert.That(entity.MeleeAttack, Is.EqualTo(10));
            graph.Add.Mods(TurnModifier.Create(entity, 2, x => x.MeleeAttack));
            Assert.That(entity.MeleeAttack, Is.EqualTo(12));

            graph.EndEncounter();

            Assert.That(entity.MeleeAttack, Is.EqualTo(10));
        }

        [Test]
        public void AddTimedMod_NextTurn_TimedModRemovedAfter3Turns()
        {
            var entity = new TestEntity();
            var graph = new Graph();

            graph.SetContext(entity);
            graph.NewEncounter();

            Assert.That(graph.Turn, Is.EqualTo(1));
            Assert.That(entity.MeleeAttack, Is.EqualTo(10));
            graph.Add.Mods(TimedModifier.Create(graph.Turn, 3, entity, 2, x => x.MeleeAttack));
            Assert.That(entity.MeleeAttack, Is.EqualTo(12));

            graph.NewTurn();
            Assert.That(graph.Turn, Is.EqualTo(2));
            Assert.That(entity.MeleeAttack, Is.EqualTo(12));

            graph.NewTurn();
            Assert.That(graph.Turn, Is.EqualTo(3));
            Assert.That(entity.MeleeAttack, Is.EqualTo(12));

            graph.NewTurn();
            Assert.That(graph.Turn, Is.EqualTo(4));
            Assert.That(entity.MeleeAttack, Is.EqualTo(12));

            graph.NewTurn();
            Assert.That(graph.Turn, Is.EqualTo(5));
            Assert.That(entity.MeleeAttack, Is.EqualTo(10));
        }
    }
}
