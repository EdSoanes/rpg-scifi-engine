using Rpg.Sys.Modifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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

            graph.Initialize(entity);

            Assert.That(entity.MeleeAttack, Is.EqualTo(10));

            graph.NewEncounter();

            graph.Mods.Add(TurnModifier.Create(entity, 2, x => x.MeleeAttack));
            Assert.That(entity.MeleeAttack, Is.EqualTo(12));

            graph.NewTurn();

            Assert.That(entity.MeleeAttack, Is.EqualTo(10));
        }

        [Test]
        public void AddTurnMod_NewEncounter_TurnModRemoved()
        {
            var entity = new TestEntity();
            var graph = new Graph();

            graph.Initialize(entity);

            Assert.That(entity.MeleeAttack, Is.EqualTo(10));
            graph.Mods.Add(TurnModifier.Create(entity, 2, x => x.MeleeAttack));
            Assert.That(entity.MeleeAttack, Is.EqualTo(12));

            graph.NewEncounter();

            Assert.That(entity.MeleeAttack, Is.EqualTo(10));
        }

        [Test]
        public void AddTurnMod_EndEncounter_TurnModRemoved()
        {
            var entity = new TestEntity();
            var graph = new Graph();

            graph.Initialize(entity);
            graph.NewEncounter();

            Assert.That(entity.MeleeAttack, Is.EqualTo(10));
            graph.Mods.Add(TurnModifier.Create(entity, 2, x => x.MeleeAttack));
            Assert.That(entity.MeleeAttack, Is.EqualTo(12));

            graph.EndEncounter();

            Assert.That(entity.MeleeAttack, Is.EqualTo(10));
        }

        [Test]
        public void AddTimedMod_NextTurn_TimedModRemovedOnTurn3()
        {
            var entity = new TestEntity();
            var graph = new Graph();

            graph.Initialize(entity);
            graph.NewEncounter();

            Assert.That(graph.Turn, Is.EqualTo(1));
            Assert.That(entity.MeleeAttack, Is.EqualTo(10));
            graph.Mods.Add(TimedModifier.Create(graph.Turn, 3, entity, 2, x => x.MeleeAttack));
            Assert.That(entity.MeleeAttack, Is.EqualTo(12));

            graph.NewTurn();
            Assert.That(graph.Turn, Is.EqualTo(2));
            Assert.That(entity.MeleeAttack, Is.EqualTo(12));

            graph.NewTurn();
            Assert.That(graph.Turn, Is.EqualTo(3));
            Assert.That(entity.MeleeAttack, Is.EqualTo(12));

            graph.NewTurn();
            Assert.That(graph.Turn, Is.EqualTo(3));
            Assert.That(entity.MeleeAttack, Is.EqualTo(12));

            graph.NewTurn();
            Assert.That(graph.Turn, Is.EqualTo(4));
            Assert.That(entity.MeleeAttack, Is.EqualTo(10));
        }
    }
}
