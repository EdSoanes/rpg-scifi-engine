﻿using Rpg.Cyborgs.Tests.Models;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rpg.Cyborgs.Actions;
using Rpg.ModObjects.Time;

namespace Rpg.Cyborgs.Tests
{
    public class TransferActionTests
    {
        private RpgGraph _graph;
        private PlayerCharacter _pc;
        private MeleeWeapon _sword;
        private RpgContainer _room;

        [SetUp]
        public void Setup()
        {
            RpgReflection.RegisterAssembly(typeof(CyborgsSystem).Assembly);

            _sword = new MeleeWeapon(WeaponFactory.SwordTemplate);
            _pc = new PlayerCharacter(ActorFactory.BennyTemplate);
            _pc.Hands.Add(_sword);

            _room = new RpgContainer("Room");
            _room.Add(_pc);

            _graph = new RpgGraph(_room);
        }

        [Test]
        public void Benny_Drops_Sword_OutsideTurn()
        {
            var transfer = _sword.CreateActionInstance(_pc, nameof(Transfer), 0);
            Assert.That(transfer, Is.Not.Null);
            Assert.That(_pc.Hands.Contains(_sword), Is.True);
            Assert.That(_room.Contains(_sword), Is.False);
            Assert.That(_pc.CurrentActions, Is.EqualTo(1));

            transfer.AutoCompleteArgs["from"] = _pc.Hands;
            transfer.AutoCompleteArgs["to"] = _graph.Context;
            transfer.AutoComplete();

            Assert.That(_pc.Hands.Contains(_sword), Is.False);
            Assert.That(_room.Contains(_sword), Is.True);
            Assert.That(_pc.CurrentActions, Is.EqualTo(1));
        }

        [Test]
        public void Benny_Drops_Sword_InsideTurn()
        {
            Assert.That(_pc.CurrentActions, Is.EqualTo(1));

            _graph.Time.SetTime(TimePoints.Encounter(1));

            var transfer = _sword.CreateActionInstance(_pc, nameof(Transfer), 0)!;
            transfer.AutoCompleteArgs["from"] = _pc.Hands;
            transfer.AutoCompleteArgs["to"] = _graph.Context;
            transfer.AutoComplete();

            Assert.That(_pc.CurrentActions, Is.EqualTo(0));
        }

        [Test]
        public void Benny_Tries_Two_Transfers_One_Turn()
        {
            Assert.That(_pc.CurrentActions, Is.EqualTo(1));

            _graph.Time.SetTime(TimePoints.Encounter(1));

            var drop = _sword.CreateActionInstance(_pc, nameof(Transfer), 0)!;
            drop.AutoCompleteArgs["from"] = _pc.Hands;
            drop.AutoCompleteArgs["to"] = _graph.Context;
            drop.AutoComplete();

            Assert.That(_pc.CurrentActions, Is.EqualTo(0));

            var pickup = _sword.CreateActionInstance(_pc, nameof(Transfer), 1)!;
            pickup.AutoCompleteArgs["from"] = _graph.Context;
            pickup.AutoCompleteArgs["to"] = _pc.Hands;

            Assert.Throws<InvalidOperationException>(() => pickup.AutoComplete());
        }
    }
}