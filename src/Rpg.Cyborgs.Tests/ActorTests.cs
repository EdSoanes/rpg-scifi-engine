﻿using Rpg.Cyborgs.States;
using Rpg.Cyborgs.Tests.Models;
using Rpg.ModObjects;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Templates;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Time;

namespace Rpg.Cyborgs.Tests
{
    internal class ActorTests
    {
        private RpgGraph _graph;
        private PlayerCharacter _pc;

        [SetUp]
        public void Setup()
        {
            RpgReflection.RegisterAssembly(typeof(CyborgsSystem).Assembly);


        }

        [Test]
        public void Benny_EnsureBaseValues()
        {
            var pc = new PlayerCharacter(ActorFactory.BennyTemplate);
            var graph = new RpgGraph(pc);

            Assert.That(pc.Name, Is.EqualTo("Benny"));
            Assert.That(pc.Strength, Is.EqualTo(-1));
            Assert.That(pc.Agility, Is.EqualTo(0));
            Assert.That(pc.Health, Is.EqualTo(1));
            Assert.That(pc.Brains, Is.EqualTo(1));
            Assert.That(pc.Insight, Is.EqualTo(0));
            Assert.That(pc.Charisma, Is.EqualTo(1));

            Assert.That(pc.StaminaPoints, Is.EqualTo(14));
            Assert.That(pc.LifePoints, Is.EqualTo(5));
            Assert.That(pc.FocusPoints, Is.EqualTo(1));
            Assert.That(pc.LuckPoints, Is.EqualTo(2));

            Assert.That(pc.Defence, Is.EqualTo(7));
            Assert.That(pc.Reactions, Is.EqualTo(7));
            Assert.That(pc.MeleeAttack, Is.EqualTo(-1));

            Assert.That(pc.IsStateOn(nameof(VeryFast)), Is.False);
        }

        [Test]
        public void Benny_EnsureActions()
        {
            var pc = new PlayerCharacter(ActorFactory.BennyTemplate);
            var graph = new RpgGraph(pc);

            var actions = pc.GetActions();
            Assert.That(actions.Count(), Is.EqualTo(6));
        }

        [Test]
        public void Benny_EnsureStates()
        {
            var pc = new PlayerCharacter(ActorFactory.BennyTemplate);
            var graph = new RpgGraph(pc);

            var states = pc.GetStates();
            Assert.That(states.Count(), Is.EqualTo(7));
            Assert.That(states.Where(x => x.IsOn).Count(), Is.EqualTo(0));
        }

        [Test]
        public void Benny_SetExhausted_EnsureStateOn()
        {
            var pc = new PlayerCharacter(ActorFactory.BennyTemplate);
            var graph = new RpgGraph(pc);

            var exhausted = pc.GetState(nameof(Exhausted));
            Assert.That(exhausted, Is.Not.Null);

            exhausted.On();
            graph.Time.TriggerEvent();

            Assert.That(exhausted.IsOn, Is.True);
            Assert.That(exhausted.IsOnManually, Is.True);
            Assert.That(exhausted.IsOnConditionally, Is.False);

            graph.Time.SetTime(TimePoints.Encounter(4));
            graph.Time.TriggerEvent();

            Assert.That(exhausted.IsOn, Is.True);
            Assert.That(exhausted.IsOnManually, Is.True);
            Assert.That(exhausted.IsOnConditionally, Is.False);

            exhausted.Off();
            graph.Time.TriggerEvent();

            Assert.That(exhausted.IsOn, Is.False);
            Assert.That(exhausted.IsOnManually, Is.False);
            Assert.That(exhausted.IsOnConditionally, Is.False);
        }

        [Test]
        public void Benny_Add4ToAgility_EnsureVeryFast()
        {
            var pc = new PlayerCharacter(ActorFactory.BennyTemplate);
            var graph = new RpgGraph(pc);

            Assert.That(pc.Actions, Is.EqualTo(1));
            Assert.That(pc.CurrentActions, Is.EqualTo(1));
            Assert.That(pc.IsStateOn(nameof(VeryFast)), Is.False);

            pc.AddMod(new PermanentMod(), x => x.Agility, 4);
            graph.Time.TriggerEvent();

            Assert.That(pc.Actions, Is.EqualTo(2));
            Assert.That(pc.CurrentActions, Is.EqualTo(2));
            Assert.That(pc.IsStateOn(nameof(VeryFast)), Is.True);
        }
    }
}