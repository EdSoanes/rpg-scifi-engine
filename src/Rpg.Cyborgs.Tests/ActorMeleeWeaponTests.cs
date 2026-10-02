using Rpg.Cyborgs.Actions;
using Rpg.Cyborgs.States;
using Rpg.Cyborgs.Tests.Models;
using Rpg.Experimental;

namespace Rpg.Cyborgs.Tests
{
    public class ActorMeleeWeaponTests
    {
        private RpgCharacterSheet _characterSheet;
        private PlayerCharacter _pc;
        private MeleeWeapon _sword;

        [SetUp]
        public void Setup()
        {
            _sword = new MeleeWeapon(WeaponFactory.SwordTemplate);
            _pc = new PlayerCharacter(ActorFactory.BennyTemplate);
            _pc.Hands.Add(_sword);

            var room = new Room();
            room.Contents.Add(_pc);

            _characterSheet = new RpgCharacterSheet(room, _pc, TestSystem.Build());
        }

        [Test]
        public void Sword_EnsureValues()
        {
            Assert.That(_sword, Is.Not.Null);
            Assert.That(_sword.Name, Is.EqualTo("Excalibur"));
            Assert.That(_sword.Damage.ToString(), Is.EqualTo("1d6"));
            Assert.That(_sword.HitBonus, Is.EqualTo(1));

            var actions = _characterSheet.GetObjectActions(_sword.Id).Select(x => x.Name).Order().ToArray();
            Assert.That(actions, Is.EqualTo(new[] { "MeleeAttack", "Transfer" }));
        }

        [Test]
        public void Sword_InHands_MeleeAttackIsPerformable()
        {
            var meleeAttack = _characterSheet.GetObjectAction(_sword.Id, nameof(MeleeAttack));

            Assert.That(meleeAttack, Is.Not.Null);
            Assert.That(meleeAttack.IsPerformable, Is.True);
        }

        [Test]
        public void PlayerCharacter_CarryingSword_Attack()
        {
            _characterSheet.Time.BeginEncounter();

            Assert.That(_characterSheet.Time.Now.Type, Is.EqualTo(TimePointType.Turn));
            Assert.That(_characterSheet.Time.Now.Count, Is.EqualTo(1));

            var activity = _characterSheet.CreateActivity(_pc.Id, _sword.Id, nameof(MeleeAttack));
            var attack = activity.CurrentActivityAction!;

            Assert.That(_characterSheet.GetPropertyValue<Dice>(attack.Id, "diceRoll").ToString(), Is.EqualTo("2d6"));

            Assert.That(attack.Cost(_characterSheet, ("actionPoints", 1), ("focusPoints", 0)), Is.True);
            Assert.That(attack.Result.Mods.Count(), Is.EqualTo(1));

            //Costs are not paid until the action is completed
            Assert.That(_pc.CurrentActionPoints, Is.EqualTo(1));

            Assert.That(attack.Perform(_characterSheet, ("targetDefence", 12)), Is.True);
            Assert.That(_characterSheet.GetPropertyValue<Dice>(attack.Id, "diceRoll").ToString(), Is.EqualTo("2d6 + 1"));
            Assert.That(_characterSheet.GetPropertyValue<Dice>(attack.Id, "targetDefence").ToString(), Is.EqualTo("12"));

            Assert.That(attack.Outcome(_characterSheet, ("diceRoll", 14)), Is.True);
            Assert.That(_characterSheet.GetPropertyValue<Dice>(attack.Id, "diceRoll").ToString(), Is.EqualTo("14"));
            Assert.That(_characterSheet.GetPropertyValue<Dice>(attack.Id, "damage").ToString(), Is.EqualTo("1d6"));

            attack.Complete();
            _characterSheet.Time.Refresh();

            Assert.That(attack.IsComplete, Is.True);
            Assert.That(_pc.CurrentActionPoints, Is.EqualTo(0));
            Assert.That(_pc.IsStateOn(nameof(MeleeAttacking)), Is.True);

            //The sword cannot be used again this turn
            var meleeAttack = _characterSheet.GetObjectAction(_sword.Id, nameof(MeleeAttack))!;
            Assert.That(meleeAttack.IsPerformable, Is.False);

            _characterSheet.Time.ToTurn(2);

            Assert.That(_pc.CurrentActionPoints, Is.EqualTo(1));
            Assert.That(_pc.IsStateOn(nameof(MeleeAttacking)), Is.False);
            Assert.That(meleeAttack.IsPerformable, Is.True);
        }
    }
}
