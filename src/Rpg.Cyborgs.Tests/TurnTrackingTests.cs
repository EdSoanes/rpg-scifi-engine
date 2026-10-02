using Rpg.Cyborgs.Actions;
using Rpg.Cyborgs.Skills.Combat;
using Rpg.Cyborgs.Skills.Movement;
using Rpg.Cyborgs.States;
using Rpg.Cyborgs.Tests.Models;
using Rpg.Experimental;

namespace Rpg.Cyborgs.Tests
{
    /// <summary>
    /// How the Cyborgs actions behave inside and outside turn tracking
    /// </summary>
    public class TurnTrackingTests
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
        public void DropSword_OutsideTurnTracking_NoActionPointCharged()
        {
            var drop = _characterSheet.CreateActivity(_pc.Id, _sword.Id, nameof(Transfer)).CurrentActivityAction!;
            drop.AutoComplete(_characterSheet, ("to", _room), ("toProp", nameof(Room.Contents)));

            Assert.That(drop.IsComplete, Is.True);
            Assert.That(_room.Contents.Contains(_sword), Is.True);

            Assert.That(_characterSheet.Time.IsTurnTracking, Is.False);
            Assert.That(_pc.CurrentActionPoints, Is.EqualTo(1));
            Assert.That(drop.SkippedCosts.Count, Is.EqualTo(1));
        }

        [Test]
        public void Run_OutsideTurnTracking_DoesNotStartIt()
        {
            var run = _characterSheet.CreateActivity(_pc.Id, _pc.Id, nameof(Run)).CurrentActivityAction!;
            run.AutoComplete(_characterSheet);

            Assert.That(run.IsComplete, Is.True);
            Assert.That(_characterSheet.Time.IsTurnTracking, Is.False);
            Assert.That(_pc.IsStateOn(nameof(Moving)), Is.False);
            Assert.That(_pc.CurrentActionPoints, Is.EqualTo(1));
        }

        [Test]
        public void Run_InsideTurnTracking_MovingForTheTurn()
        {
            _characterSheet.BeginTurnTracking();

            var run = _characterSheet.CreateActivity(_pc.Id, _pc.Id, nameof(Run)).CurrentActivityAction!;
            run.AutoComplete(_characterSheet);

            Assert.That(_pc.IsStateOn(nameof(Moving)), Is.True);
            Assert.That(_pc.CurrentActionPoints, Is.EqualTo(0));

            _characterSheet.NextTurn();

            Assert.That(_pc.IsStateOn(nameof(Moving)), Is.False);
            Assert.That(_pc.CurrentActionPoints, Is.EqualTo(1));
        }

        [Test]
        public void Aim_OutsideTurnTracking_StartsIt_AndCostsTheAction()
        {
            var aim = _characterSheet.CreateActivity(_pc.Id, _pc.Id, nameof(Aim)).CurrentActivityAction!;
            aim.AutoComplete(_characterSheet);

            //The aim bonus lasts a turn, so the clock is now running
            Assert.That(aim.IsComplete, Is.True);
            Assert.That(_characterSheet.Time.IsTurnTracking, Is.True);
            Assert.That(_characterSheet.Time.Turn, Is.EqualTo(1));

            Assert.That(_pc.IsStateOn(nameof(Aiming)), Is.True);
            Assert.That(_pc.RangedAimBonus, Is.EqualTo(2));
            Assert.That(_pc.CurrentActionPoints, Is.EqualTo(0));
            Assert.That(aim.SkippedCosts.Count, Is.EqualTo(0));
            Assert.That(_characterSheet.GetTurnTrackingReport().IsNeeded, Is.True);

            _characterSheet.NextTurn();

            Assert.That(_pc.IsStateOn(nameof(Aiming)), Is.False);
            Assert.That(_pc.RangedAimBonus, Is.EqualTo(0));
            Assert.That(_pc.CurrentActionPoints, Is.EqualTo(1));
            Assert.That(_characterSheet.GetTurnTrackingReport().IsNeeded, Is.False);

            //Nothing turn based remains but it is the players who end turn tracking
            Assert.That(_characterSheet.Time.IsTurnTracking, Is.True);
        }

        [Test]
        public void TakeDamage_OutsideTurnTracking_DamageApplied_NoTurnTracking()
        {
            var takeDamage = _characterSheet.CreateActivity(_pc.Id, _pc.Id, nameof(TakeDamage)).CurrentActivityAction!;

            Assert.That(takeDamage.Outcome(_characterSheet, ("damage", 10)), Is.True);
            takeDamage.Complete(_characterSheet);

            Assert.That(_characterSheet.Time.IsTurnTracking, Is.False);
            Assert.That(_pc.CurrentStaminaPoints, Is.EqualTo(4));

            _characterSheet.TriggerTimeEvent("TimePasses");

            Assert.That(_pc.CurrentStaminaPoints, Is.EqualTo(4));
            Assert.That(_characterSheet.ObjectExists(takeDamage.Id), Is.False);
        }

        [Test]
        public void Attack_ThenGoBackATurn_AttackIsUndone()
        {
            _characterSheet.BeginTurnTracking();
            _characterSheet.NextTurn();

            var attack = _characterSheet.CreateActivity(_pc.Id, _sword.Id, nameof(MeleeAttack)).CurrentActivityAction!;
            Assert.That(attack.Cost(_characterSheet, ("actionPoints", 1), ("focusPoints", 1)), Is.True);
            Assert.That(attack.Perform(_characterSheet, ("targetDefence", 12)), Is.True);
            Assert.That(attack.Outcome(_characterSheet, ("diceRoll", 14)), Is.True);
            attack.Complete(_characterSheet);

            var takeDamage = _characterSheet.CreateActivity(_pc.Id, _pc.Id, nameof(TakeDamage)).CurrentActivityAction!;
            Assert.That(takeDamage.Outcome(_characterSheet, ("damage", 6)), Is.True);
            takeDamage.Complete(_characterSheet);

            Assert.That(_pc.CurrentActionPoints, Is.EqualTo(0));
            Assert.That(_pc.CurrentFocusPoints, Is.EqualTo(0));
            Assert.That(_pc.CurrentStaminaPoints, Is.EqualTo(8));
            Assert.That(_pc.IsStateOn(nameof(MeleeAttacking)), Is.True);

            _characterSheet.NextTurn();
            Assert.That(_pc.CurrentStaminaPoints, Is.EqualTo(8));

            //The GM replays turn 2
            Assert.That(_characterSheet.RewindToTurn(2), Is.True);
            var pc = (_characterSheet.Actor as PlayerCharacter)!;

            Assert.That(_characterSheet.Time.Turn, Is.EqualTo(2));
            Assert.That(pc.CurrentActionPoints, Is.EqualTo(1));
            Assert.That(pc.CurrentFocusPoints, Is.EqualTo(1));
            Assert.That(pc.CurrentStaminaPoints, Is.EqualTo(14));
            Assert.That(pc.IsStateOn(nameof(MeleeAttacking)), Is.False);
            Assert.That(pc.Hands.Count, Is.EqualTo(1));
            Assert.That(_characterSheet.GetObjectAction(pc.Hands[0].Id, nameof(MeleeAttack))?.IsPerformable, Is.True);
        }
    }
}
