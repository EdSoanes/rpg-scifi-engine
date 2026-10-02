using Rpg.Cyborgs.Actions;
using Rpg.Cyborgs.Tests.Models;
using Rpg.Experimental;
using Rpg.Experimental.Reflection.Args;

namespace Rpg.Cyborgs.Tests
{
    public class TransferActionTests
    {
        private RpgCharacterSheet _characterSheet;
        private PlayerCharacter _pc;
        private MeleeWeapon _sword;
        private Room _room;

        [SetUp]
        public void Setup()
        {
            _sword = new MeleeWeapon(WeaponFactory.SwordTemplate);
            _pc = new PlayerCharacter(ActorFactory.BennyTemplate);
            _pc.Hands.Add(_sword);

            _room = new Room();
            _room.Contents.Add(_pc);

            _characterSheet = new RpgCharacterSheet(_room, _pc, TestSystem.Build());
        }

        [Test]
        public void Benny_Transfers_Sword_EnsureInitialValues()
        {
            var transfer = _characterSheet.GetObjectAction(_sword.Id, nameof(Transfer));
            Assert.That(transfer, Is.Not.Null);
            Assert.That(transfer.IsPerformable, Is.True);

            var activity = _characterSheet.CreateActivity(_pc.Id, _sword.Id, nameof(Transfer));
            var action = activity.CurrentActivityAction;

            Assert.That(action, Is.Not.Null);
            Assert.That(_pc.Hands.Contains(_sword), Is.True);
            Assert.That(_room.Contents.Contains(_sword), Is.False);
            Assert.That(_pc.CurrentActionPoints, Is.EqualTo(1));
        }

        [Test]
        public void Benny_Drops_Sword()
        {
            var activity = _characterSheet.CreateActivity(_pc.Id, _sword.Id, nameof(Transfer));
            var transfer = activity.CurrentActivityAction!;

            Assert.That(transfer.Cost(_characterSheet), Is.True);
            Assert.That(transfer.Perform(_characterSheet), Is.True);

            Assert.That(transfer.OutcomeMethod.Args.IsComplete(), Is.False);
            Assert.That(transfer.Outcome(_characterSheet), Is.False);
            Assert.That(transfer.Outcome(_characterSheet, ("to", _room), ("toProp", nameof(Room.Contents))), Is.True);

            transfer.Complete(_characterSheet);
            _characterSheet.Time.Refresh();

            Assert.That(transfer.IsComplete, Is.True);
            Assert.That(_pc.Hands.Contains(_sword), Is.False);
            Assert.That(_room.Contents.Contains(_sword), Is.True);
            Assert.That(_room.Contents.Contains(_pc), Is.True);
        }

        [Test]
        public void Benny_Drops_Sword_InvalidTarget_Fails()
        {
            var activity = _characterSheet.CreateActivity(_pc.Id, _sword.Id, nameof(Transfer));
            var transfer = activity.CurrentActivityAction!;

            Assert.That(transfer.Cost(_characterSheet), Is.True);
            Assert.That(transfer.Outcome(_characterSheet, ("to", _room), ("toProp", "NotAProp")), Is.False);

            _characterSheet.Time.Refresh();

            Assert.That(_pc.Hands.Contains(_sword), Is.True);
            Assert.That(_room.Contents.Contains(_sword), Is.False);
        }

        [Test]
        public void Benny_Drops_Sword_OnTurn2_CostsAnAction()
        {
            Assert.That(_pc.CurrentActionPoints, Is.EqualTo(1));

            _characterSheet.Time.ToTurn(2);

            var activity = _characterSheet.CreateActivity(_pc.Id, _sword.Id, nameof(Transfer));
            var transfer = activity.CurrentActivityAction!;

            Assert.That(transfer.CanAutoComplete, Is.False);

            Assert.That(transfer.Cost(_characterSheet), Is.True);
            Assert.That(transfer.Perform(_characterSheet), Is.True);
            Assert.That(transfer.Outcome(_characterSheet, ("to", _room), ("toProp", nameof(Room.Contents))), Is.True);

            transfer.Complete(_characterSheet);
            _characterSheet.Time.Refresh();

            Assert.That(_pc.CurrentActionPoints, Is.EqualTo(0));
            Assert.That(_pc.Hands.Contains(_sword), Is.False);
            Assert.That(_room.Contents.Contains(_sword), Is.True);

            //No action points left so nothing else can be transferred this turn
            var transferAction = _characterSheet.GetObjectAction(_sword.Id, nameof(Transfer))!;
            Assert.That(transferAction.IsPerformable, Is.False);

            _characterSheet.Time.ToTurn(3);

            Assert.That(_pc.CurrentActionPoints, Is.EqualTo(1));
            Assert.That(transferAction.IsPerformable, Is.True);
            Assert.That(_pc.Hands.Contains(_sword), Is.False);
            Assert.That(_room.Contents.Contains(_sword), Is.True);
        }

        [Test]
        public void Benny_Drops_Sword_AutoComplete()
        {
            _characterSheet.Time.BeginEncounter();

            var activity = _characterSheet.CreateActivity(_pc.Id, _sword.Id, nameof(Transfer));
            var transfer = activity.CurrentActivityAction!;

            transfer.AutoComplete(_characterSheet, ("to", _room), ("toProp", nameof(Room.Contents)));
            _characterSheet.Time.Refresh();

            Assert.That(transfer.IsComplete, Is.True);
            Assert.That(_pc.CurrentActionPoints, Is.EqualTo(0));
            Assert.That(_pc.Hands.Contains(_sword), Is.False);
            Assert.That(_room.Contents.Contains(_sword), Is.True);
        }

        [Test]
        public void Benny_Drops_Sword_Then_PicksItUp()
        {
            _characterSheet.Time.BeginEncounter();

            var drop = _characterSheet.CreateActivity(_pc.Id, _sword.Id, nameof(Transfer)).CurrentActivityAction!;
            drop.AutoComplete(_characterSheet, ("to", _room), ("toProp", nameof(Room.Contents)));
            _characterSheet.Time.Refresh();

            Assert.That(_room.Contents.Contains(_sword), Is.True);

            _characterSheet.Time.ToTurn(2);

            var pickUp = _characterSheet.CreateActivity(_pc.Id, _sword.Id, nameof(Transfer)).CurrentActivityAction!;
            Assert.That(pickUp.Id, Is.Not.EqualTo(drop.Id));

            pickUp.AutoComplete(_characterSheet, ("to", _pc), ("toProp", nameof(Actor.Hands)));
            _characterSheet.Time.Refresh();

            Assert.That(pickUp.IsComplete, Is.True);
            Assert.That(_pc.Hands.Contains(_sword), Is.True);
            Assert.That(_room.Contents.Contains(_sword), Is.False);
        }
    }
}
