﻿using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Tests.Models;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Time.Templates;
using System.Reflection;

namespace Rpg.ModObjects.Tests
{
    public class TurnTests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeScan.RegisterAssembly(this.GetType().Assembly);
        }

        [Test]
        public void AddTurnMod_IncrementTurn_TurnModRemoved()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));

            graph.Time.SetTime(TimePoints.BeginningOfEncounter);

            entity.AddMod(new TurnMod(), x => x.Melee, 2);
            graph.Time.TriggerEvent();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(6));

            graph.Time.IncreaseTick();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));
        }

        [Test]
        public void AddTurnMod_RepeatSameTurn_TurnModNotRemoved()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));

            graph.Time.SetTime(TimePoints.BeginningOfEncounter);

            entity.AddMod(new TurnMod(), x => x.Melee, 2);
            graph.Time.TriggerEvent();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(6));

            graph.Time.SetTick(1);

            Assert.That(entity.Melee.Roll(), Is.EqualTo(6));
        }

        [Test]
        public void AddTurnMod_RevertTurn_TurnModReapplied()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));

            graph.Time.SetTime(TimePoints.BeginningOfEncounter);

            entity.AddMod(new TurnMod(), x => x.Melee, 2);
            graph.Time.TriggerEvent();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(6));

            graph.Time.IncreaseTick();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));

            graph.Time.DecreaseTick();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(6));
        }

        [Test]
        public void AddTurnMod_NewEncounter_TurnModRemoved()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            var modCount = graph.GetActiveMods().Count();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));

            graph.Time.SetTime(TimePoints.BeginningOfEncounter);

            entity.AddMod(new TurnMod(), x => x.Melee, 2);
            graph.Time.TriggerEvent();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(6));

            graph.Time.SetTime(TimePoints.BeginningOfEncounter);

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));
            Assert.That(graph.GetActiveMods().Count(), Is.EqualTo(modCount));
        }

        [Test]
        public void AddTurnMod_EndEncounter_TurnModRemoved()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            var modCount = graph.GetActiveMods().Count();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));

            graph.Time.SetTime(TimePoints.BeginningOfEncounter);

            entity.AddMod(new TurnMod(), x => x.Melee, 2);
            graph.Time.TriggerEvent();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(6));

            graph.Time.SetTime(TimePoints.EndOfEncounter);

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));
            Assert.That(graph.GetActiveMods().Count(), Is.EqualTo(modCount));
        }

        [Test]
        public void AddTimedMod_NextTurn_TimedModRemovedAfter3Turns()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));

            graph.Time.SetTime(TimePoints.BeginningOfEncounter);

            entity.AddMod(new TurnMod(3), x => x.Melee, 2);
            graph.Time.TriggerEvent();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(6));

            //Turn 2
            graph.Time.IncreaseTick();
            Assert.That(entity.Melee.Roll(), Is.EqualTo(6));

            //Turn 3
            graph.Time.IncreaseTick();
            Assert.That(entity.Melee.Roll(), Is.EqualTo(6));

            //Turn 4
            graph.Time.IncreaseTick();
            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));
        }

        [Test]
        public void AddTimedMod_RewindTurn_TimedModValid()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));

            graph.Time.SetTime(TimePoints.BeginningOfEncounter);

            entity.AddMod(new TurnMod(3), x => x.Melee, 2);
            graph.Time.TriggerEvent();

            Assert.That(entity.Melee.Roll(), Is.EqualTo(6));

            //Set to Turn 4
            graph.Time.SetTick(4);
            Assert.That(entity.Melee.Roll(), Is.EqualTo(4));

            //Set to Turn 3
            graph.Time.SetTick(3);
            Assert.That(entity.Melee.Roll(), Is.EqualTo(6));
        }
    }
}
