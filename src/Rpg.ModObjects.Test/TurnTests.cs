using Newtonsoft.Json;
using Rpg.ModObjects.Modifiers;
using Rpg.ModObjects.Tests.Models;
using System.Reflection;

namespace Rpg.ModObjects.Tests
{
    public class TurnTests
    {
        [SetUp]
        public void Setup()
        {
            ModGraphExtensions.RegisterAssembly(Assembly.GetExecutingAssembly());
        }

        [Test]
        public void AddTurnMod_IncrementTurn_TurnModRemoved()
        {
            var entity = new ModdableEntity();
            var graph = entity.BuildGraph();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));

            graph.NewEncounter();

            entity.AddMod(TurnMod.Create(entity, x => x.Melee, 2));
            entity.TriggerUpdate(x => x.Melee);

            Assert.That(entity.Melee.Roll(), Is.EqualTo(6));

            graph.NewTurn();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));
        }

        [Test]
        public void AddTurnMod_RepeatSameTurn_TurnModNotRemoved()
        {
            var entity = new ModdableEntity();
            var graph = entity.BuildGraph();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));

            graph.NewEncounter();

            entity.AddMod(TurnMod.Create(entity, x => x.Melee, 2));
            entity.TriggerUpdate(x => x.Melee);

            Assert.That(entity.Melee.Roll(), Is.EqualTo(6));

            graph.SetTurn(1);

            Assert.That(entity.Melee.Roll(), Is.EqualTo(6));
        }

        [Test]
        public void AddTurnMod_RevertTurn_TurnModReapplied()
        {
            var entity = new ModdableEntity();
            var graph = entity.BuildGraph();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));

            graph.NewEncounter();

            entity.AddMod(TurnMod.Create(entity, x => x.Melee, 2));
            entity.TriggerUpdate(x => x.Melee);

            Assert.That(entity.Melee.Roll(), Is.EqualTo(6));

            graph.NewTurn();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));

            graph.PrevTurn();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(6));
        }

        [Test]
        public void AddTurnMod_NewEncounter_TurnModRemoved()
        {
            var entity = new ModdableEntity();
            var graph = entity.BuildGraph();
            var modCount = graph.GetMods().Count();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));

            graph.NewEncounter();

            entity.AddMod(TurnMod.Create(entity, x => x.Melee, 2));
            entity.TriggerUpdate(x => x.Melee);

            Assert.That(entity.Melee.Roll(), Is.EqualTo(6));

            graph.NewEncounter();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));
            Assert.That(graph.GetMods().Count(), Is.EqualTo(modCount));
        }

        [Test]
        public void AddTurnMod_EndEncounter_TurnModRemoved()
        {
            var entity = new ModdableEntity();
            var graph = entity.BuildGraph();
            var modCount = graph.GetMods().Count();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));

            graph.NewEncounter();

            entity.AddMod(TurnMod.Create(entity, x => x.Melee, 2));
            entity.TriggerUpdate(x => x.Melee);

            Assert.That(entity.Melee.Roll(), Is.EqualTo(6));

            graph.EndEncounter();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));
            Assert.That(graph.GetMods().Count(), Is.EqualTo(modCount));
        }

        [Test]
        public void AddTimedMod_NextTurn_TimedModRemovedAfter3Turns()
        {
            var entity = new ModdableEntity();
            var graph = entity.BuildGraph();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));

            graph.NewEncounter();

            entity.AddMod(TimedMod.Create(1, 3, entity, x => x.Melee, 2));
            entity.TriggerUpdate(x => x.Melee);

            Assert.That(entity.Melee.Roll(), Is.EqualTo(6));

            //Turn 2
            graph.NewTurn();
            Assert.That(entity.Melee.Roll(), Is.EqualTo(6));

            //Turn 3
            graph.NewTurn();
            Assert.That(entity.Melee.Roll(), Is.EqualTo(6));

            //Turn 4
            graph.NewTurn();
            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));
        }

        [Test]
        public void AddTimedMod_RewindTurn_TimedModValid()
        {
            var entity = new ModdableEntity();
            var graph = entity.BuildGraph();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));

            graph.NewEncounter();

            entity.AddMod(TimedMod.Create(1, 3, entity, x => x.Melee, 2));
            entity.TriggerUpdate(x => x.Melee);

            Assert.That(entity.Melee.Roll(), Is.EqualTo(6));

            //Set to Turn 4
            graph.SetTurn(4);
            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));

            //Set to Turn 3
            graph.SetTurn(3);
            Assert.That(entity.Melee.Roll(), Is.EqualTo(6));
        }
    }
}
