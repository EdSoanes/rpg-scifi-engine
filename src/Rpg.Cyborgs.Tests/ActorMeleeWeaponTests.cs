﻿using Rpg.Cyborgs.Actions;
using Rpg.Cyborgs.Tests.Models;
using Rpg.ModObjects;
using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;

namespace Rpg.Cyborgs.Tests
{
    public class ActorMeleeWeaponTests
    {
        private RpgGraph _graph;
        private PlayerCharacter _pc;
        private MeleeWeapon _sword;

        [SetUp]
        public void Setup()
        {
            RpgTypeScan.RegisterAssembly(typeof(CyborgsSystem).Assembly);

            _sword = new MeleeWeapon(WeaponFactory.SwordTemplate);
            _pc = new PlayerCharacter(ActorFactory.BennyTemplate);
            _pc.Hands.Add(_sword);

            var room = new Room();
            room.Contents.Add(_pc);

            _graph = new RpgGraph(room);
        }

        [Test]
        public void Sword_EnsureValues()
        {
            Assert.That(_sword, Is.Not.Null);
            Assert.That(_sword.Name, Is.EqualTo("Excalibur"));
            Assert.That(_sword.Damage.ToString(), Is.EqualTo("1d6"));
            Assert.That(_sword.HitBonus, Is.EqualTo(1));
        }

        [Test]
        public void PlayerCharacter_CarryingSword_Attack()
        {
            var activity = _graph.CreateActivity(_pc, _sword, nameof(MeleeAttack));
            var attack = activity.ActionInstance;

            _graph.Time.Transition(PointInTimeType.EncounterBegins);
            Assert.That(_graph.Time.Now.Type, Is.EqualTo(PointInTimeType.Turn));
            Assert.That(_graph.Time.Now.Count, Is.EqualTo(1));

            var costs = activity.Cost();
            Assert.That(costs.Mods.Count(), Is.EqualTo(1));

            _graph.AddModSets(costs);
            _graph.Time.TriggerEvent();

            Assert.That(_pc.CurrentActionPoints, Is.EqualTo(_pc.ActionPoints - 1));

            activity
                .SetActArg("focusPoints", 0)
                .SetActArg("targetDefence", 12)
                .Act();

            Assert.That(activity.GetActivityProp("diceRoll")?.ToString(), Is.EqualTo("2d6"));
            Assert.That(activity.GetActivityProp("target")?.ToString(), Is.EqualTo("12"));

            activity
                .ActivityResultMod("diceRoll", "Result", 14)
                .ActivityResultMod("target", "Result", 12);

            Assert.That(activity.GetActivityProp("diceRoll")?.ToString(), Is.EqualTo("14"));
            Assert.That(activity.GetActivityProp("target")?.ToString(), Is.EqualTo("12"));
            
            activity.Outcome();
            Assert.That(activity.GetActivityProp("damage")?.ToString(), Is.EqualTo("1d6 - 1"));

            activity.Complete();

        }
    }
}
