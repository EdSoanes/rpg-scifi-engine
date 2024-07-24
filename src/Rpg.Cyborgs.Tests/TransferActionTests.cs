using Rpg.Cyborgs.Tests.Models;
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
        private Room _room;

        [SetUp]
        public void Setup()
        {
            RpgReflection.RegisterAssembly(typeof(CyborgsSystem).Assembly);

            _sword = new MeleeWeapon(WeaponFactory.SwordTemplate);
            _pc = new PlayerCharacter(ActorFactory.BennyTemplate);
            _pc.Hands.Add(_sword);

            _room = new Room();
            _room.Contents.Add(_pc);

            _graph = new RpgGraph(_room);
        }

        [Test]
        public void Benny_Drops_Sword_OutsideTurn()
        {
            var transfer = _sword.CreateActionInstance(_pc, nameof(Transfer), 0);
            Assert.That(transfer, Is.Not.Null);
            Assert.That(_pc.Hands.Contains(_sword), Is.True);
            Assert.That(_room.Contents.Contains(_sword), Is.False);
            Assert.That(_pc.CurrentActionPoints, Is.EqualTo(1));

            transfer.AutoCompleteArgs!
                .Set("from", _pc.Hands)
                .Set("to", (_graph.Context as Room)!.Contents);

            transfer.AutoComplete();

            Assert.That(_pc.Hands.Contains(_sword), Is.False);
            Assert.That(_room.Contents.Contains(_sword), Is.True);
            Assert.That(_pc.CurrentActionPoints, Is.EqualTo(1));
        }

        [Test]
        public void Benny_Drops_Sword_InsideTurn()
        {
            Assert.That(_pc.CurrentActionPoints, Is.EqualTo(1));

            _graph.Time.SetTime(TimePoints.Encounter(1));

            var transfer = _sword.CreateActionInstance(_pc, nameof(Transfer), 0)!;

            transfer.AutoCompleteArgs!
                .Set("from", _pc.Hands)
                .Set("to", (_graph.Context as Room)!.Contents);

            transfer.AutoComplete();

            Assert.That(_pc.CurrentActionPoints, Is.EqualTo(0));
        }

        [Test]
        public void Benny_Tries_Two_Transfers_One_Turn()
        {
            Assert.That(_pc.CurrentActionPoints, Is.EqualTo(1));

            _graph.Time.SetTime(TimePoints.Encounter(1));

            var drop = _sword.CreateActionInstance(_pc, nameof(Transfer), 0)!;

            drop.AutoCompleteArgs!
                .Set("from", _pc.Hands)
                .Set("to", (_graph.Context as Room)!.Contents);

            drop.AutoComplete();

            Assert.That(_pc.CurrentActionPoints, Is.EqualTo(0));

            var pickup = _sword.CreateActionInstance(_pc, nameof(Transfer), 1)!;

            pickup.AutoCompleteArgs!
                .Set("from", (_graph.Context as Room)!.Contents)
                .Set("to", _pc.Hands);

            Assert.Throws<InvalidOperationException>(() => pickup.AutoComplete());
        }
    }
}
