﻿using Rpg.Cyborgs.States;
using Rpg.Cyborgs.Tests.Models;
using Rpg.ModObjects;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Mods;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Server.Json;
using Rpg.ModObjects.Time;

namespace Rpg.Cyborgs.Tests
{
    internal class ActorTests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeScan.RegisterAssembly(typeof(CyborgsSystem).Assembly);
        }

        [Test]
        public void Benny_EnsureBaseValues()
        {
            var pc = new PlayerCharacter(ActorFactory.BennyTemplate);
            var graph = new RpgGraph(pc);

            Assert.That(pc.Name, Is.EqualTo("Benny"));
            Assert.That(pc.Strength.Value, Is.EqualTo(-1));
            Assert.That(pc.Agility.Value, Is.EqualTo(0));
            Assert.That(pc.Health.Value, Is.EqualTo(1));
            Assert.That(pc.Brains.Value, Is.EqualTo(1));
            Assert.That(pc.Insight.Value, Is.EqualTo(0));
            Assert.That(pc.Charisma.Value, Is.EqualTo(1));

            Assert.That(pc.StaminaPoints, Is.EqualTo(14));
            Assert.That(pc.LifePoints, Is.EqualTo(5));
            Assert.That(pc.FocusPoints, Is.EqualTo(1));
            Assert.That(pc.LuckPoints, Is.EqualTo(2));

            Assert.That(pc.Defence.Value, Is.EqualTo(7));
            Assert.That(pc.ArmourRating.Value, Is.EqualTo(6));
            Assert.That(pc.Reactions.Value, Is.EqualTo(7));
            Assert.That(pc.MeleeAttack.Value, Is.EqualTo(-1));
            Assert.That(pc.RangedAttack.Value, Is.EqualTo(0));

            Assert.That(pc.ActionPoints, Is.EqualTo(1));
            Assert.That(pc.IsStateOn(nameof(VeryFast)), Is.False);
        }

        [Test]
        public void Benny_EnsureActions()
        {
            var pc = new PlayerCharacter(ActorFactory.BennyTemplate);
            var graph = new RpgGraph(pc);

            Assert.That(pc.ActionTemplates.Count(), Is.EqualTo(7));
        }

        [Test]
        public void Benny_EnsureStates()
        {
            var pc = new PlayerCharacter(ActorFactory.BennyTemplate);
            var graph = new RpgGraph(pc);

            Assert.That(pc.States.Count(), Is.EqualTo(10));
            Assert.That(pc.States.Values.Where(x => x.IsOn).Count(), Is.EqualTo(0));
        }

        [Test]
        public void Benny_SetExhausted_EnsureStateOn()
        {
            var pc = new PlayerCharacter(ActorFactory.BennyTemplate);
            var graph = new RpgGraph(pc);

            var exhausted = pc.GetState(nameof(Exhausted));
            Assert.That(exhausted, Is.Not.Null);

            exhausted.Activate();
            graph.Time.TriggerEvent();

            Assert.That(exhausted.IsOn, Is.True);
            Assert.That(exhausted.IsOnManually, Is.True);
            Assert.That(exhausted.IsOnTimed, Is.False);

            graph.Time.Transition(PointInTimeType.Turn, 4);
            graph.Time.TriggerEvent();

            Assert.That(exhausted.IsOn, Is.True);
            Assert.That(exhausted.IsOnManually, Is.True);
            Assert.That(exhausted.IsOnTimed, Is.False);

            exhausted.Deactivate();
            graph.Time.TriggerEvent();

            Assert.That(exhausted.IsOn, Is.False);
            Assert.That(exhausted.IsOnManually, Is.False);
            Assert.That(exhausted.IsOnTimed, Is.False);
        }

        [Test]
        public void Benny_Add4ToAgility_EnsureVeryFast()
        {
            var pc = new PlayerCharacter(ActorFactory.BennyTemplate);
            var graph = new RpgGraph(pc);

            Assert.That(pc.ActionPoints, Is.EqualTo(1));
            Assert.That(pc.CurrentActionPoints, Is.EqualTo(1));
            Assert.That(pc.IsStateOn(nameof(VeryFast)), Is.False);

            pc.AddMod(new Permanent(), x => x.Agility.Value, 4);
            graph.Time.TriggerEvent();

            Assert.That(pc.ActionPoints, Is.EqualTo(2));
            Assert.That(pc.CurrentActionPoints, Is.EqualTo(2));
            Assert.That(pc.IsStateOn(nameof(VeryFast)), Is.True);
        }

        [Test]
        public void Benny_CarriesSword_EnsureSerialization()
        {
            var sword = new MeleeWeapon(WeaponFactory.SwordTemplate);
            var pc = new PlayerCharacter(ActorFactory.BennyTemplate);
            pc.Hands.Add(sword);

            var graph = new RpgGraph(pc);
            var json = RpgJson.Serialize(pc);

            Assert.That(json, Is.Not.Null);
        }
    }
}
