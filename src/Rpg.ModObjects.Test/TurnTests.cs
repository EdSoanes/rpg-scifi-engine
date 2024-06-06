using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Tests.Models;
using Rpg.ModObjects.Time;
using System.Reflection;

namespace Rpg.ModObjects.Tests
{
    public class TurnTests
    {
        [SetUp]
        public void Setup()
        {
            RpgGraphExtensions.RegisterAssembly(Assembly.GetExecutingAssembly());
        }

        [Test]
        public void AddTurnMod_IncrementTurn_TurnModRemoved()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));

            graph.Time.NewEncounter();

            entity.AddMod(new TurnMod(), x => x.Melee, 2);
            graph.Time.TriggerEvent();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(6));

            graph.Time.NewTurn();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));
        }

        [Test]
        public void AddTurnMod_RepeatSameTurn_TurnModNotRemoved()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));

            graph.Time.NewEncounter();

            entity.AddMod(new TurnMod(), x => x.Melee, 2);
            graph.Time.TriggerEvent();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(6));

            graph.Time.SetTurn(1);

            Assert.That(entity.Melee.Roll(), Is.EqualTo(6));
        }

        [Test]
        public void AddTurnMod_RevertTurn_TurnModReapplied()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));

            graph.Time.NewEncounter();

            entity.AddMod(new TurnMod(), x => x.Melee, 2);
            graph.Time.TriggerEvent();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(6));

            graph.Time.NewTurn();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));

            graph.Time.PrevTurn();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(6));
        }

        [Test]
        public void AddTurnMod_NewEncounter_TurnModRemoved()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            var modCount = graph.GetMods().Count();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));

            graph.Time.NewEncounter();

            entity.AddMod(new TurnMod(), x => x.Melee, 2);
            graph.Time.TriggerEvent();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(6));

            graph.Time.NewEncounter();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));
            Assert.That(graph.GetMods().Count(), Is.EqualTo(modCount));
        }

        [Test]
        public void AddTurnMod_EndEncounter_TurnModRemoved()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            var modCount = graph.GetMods().Count();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));

            graph.Time.NewEncounter();

            entity.AddMod(new TurnMod(), x => x.Melee, 2);
            graph.Time.TriggerEvent();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(6));

            graph.Time.EndEncounter();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));
            Assert.That(graph.GetMods().Count(), Is.EqualTo(modCount));
        }

        [Test]
        public void AddTimedMod_NextTurn_TimedModRemovedAfter3Turns()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));

            graph.Time.NewEncounter();

            entity.AddMod(new TurnMod(3), x => x.Melee, 2);
            graph.Time.TriggerEvent();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(6));

            //Turn 2
            graph.Time.NewTurn();
            Assert.That(entity.Melee.Roll(), Is.EqualTo(6));

            //Turn 3
            graph.Time.NewTurn();
            Assert.That(entity.Melee.Roll(), Is.EqualTo(6));

            //Turn 4
            graph.Time.NewTurn();
            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));
        }

        [Test]
        public void AddTimedMod_RewindTurn_TimedModValid()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));

            graph.Time.NewEncounter();

            entity.AddMod(new TurnMod(3), x => x.Melee, 2);
            graph.Time.TriggerEvent();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(6));

            //Set to Turn 4
            graph.Time.SetTurn(4);
            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));

            //Set to Turn 3
            graph.Time.SetTurn(3);
            Assert.That(entity.Melee.Roll(), Is.EqualTo(6));
        }
    }
}
