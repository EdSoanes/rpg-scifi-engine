using Rpg.Cyborgs.Actions;
using Rpg.Cyborgs.Tests.Models;
using Rpg.Experimental;

namespace Rpg.Cyborgs.Tests
{
    /// <summary>
    /// The Cyborgs actions with real dice and with app dice. Nothing is rolled until someone asks.
    /// </summary>
    public class DiceRollTests
    {
        private RpgCharacterSheet _characterSheet;
        private PlayerCharacter _pc;
        private MeleeWeapon _sword;
        private Armour _vest;
        private FixedDiceRoller _roller;

        [SetUp]
        public void Setup()
        {
            _sword = new MeleeWeapon(WeaponFactory.SwordTemplate);
            _vest = new Armour(ArmourFactory.VestTemplate);
            _pc = new PlayerCharacter(ActorFactory.BennyTemplate);
            _pc.Hands.Add(_sword);

            var room = new Room();
            room.Contents.Add(_pc);
            room.Contents.Add(_vest);

            _characterSheet = new RpgCharacterSheet(room, _pc, TestSystem.Build());
        }

        private void UseDice(params int[] results)
        {
            _roller = new FixedDiceRoller(results);
            _characterSheet.DiceRoller = _roller;
        }

        private RpgActivityAction Start(string ownerId, string actionName)
            => _characterSheet.CreateActivity(_pc.Id, ownerId, actionName).CurrentActivityAction!;

        [Test]
        public void MeleeAttack_RealDice_WaitsForThePlayer()
        {
            UseDice();
            _characterSheet.BeginTurnTracking();

            var attack = Start(_sword.Id, nameof(MeleeAttack));

            Assert.That(attack.Cost(_characterSheet, ("actionPoints", 1), ("focusPoints", 0)), Is.True);
            Assert.That(attack.Perform(_characterSheet, ("targetDefence", 12)), Is.True);

            //The attack roll has not been made, so the outcome cannot be worked out
            Assert.That(attack.Outcome(_characterSheet), Is.False);

            var pending = _characterSheet.GetPendingRolls();
            Assert.That(pending.Length, Is.EqualTo(1));
            Assert.That(pending[0].ObjectId, Is.EqualTo(attack.Id));
            Assert.That(pending[0].Prop, Is.EqualTo("diceRoll"));
            Assert.That(pending[0].Expression.ToString(), Is.EqualTo("2d6 + 1"));
            Assert.That(pending[0].DicePart.ToString(), Is.EqualTo("2d6"));
            Assert.That(pending[0].ActionName, Is.EqualTo(nameof(MeleeAttack)));

            //Waiting does not roll
            _characterSheet.Time.Refresh();
            _characterSheet.Time.Refresh();
            Assert.That(_roller.Rolled, Is.EqualTo(0));
            Assert.That(_characterSheet.GetPropertyValue<Dice>(attack.Id, "diceRoll").ToString(), Is.EqualTo("2d6 + 1"));

            //The player rolls 11 on real dice. The sword's hit bonus is added.
            _characterSheet.SetRoll(attack, "diceRoll", 11);

            Assert.That(attack.Outcome(_characterSheet), Is.True);
            Assert.That(_characterSheet.GetPropertyValue<Dice>(attack.Id, "diceRoll").ToString(), Is.EqualTo("12"));
            Assert.That(_characterSheet.GetPendingRolls(), Is.Empty);

            attack.Complete(_characterSheet);
            Assert.That(attack.IsComplete, Is.True);
            Assert.That(_roller.Rolled, Is.EqualTo(0));
        }

        [Test]
        public void MeleeAttack_AppDice_RolledOnceAndKept()
        {
            UseDice(4, 6);
            _characterSheet.RollMode = RpgRollMode.App;
            _characterSheet.BeginTurnTracking();

            var attack = Start(_sword.Id, nameof(MeleeAttack));

            Assert.That(attack.Cost(_characterSheet, ("actionPoints", 1), ("focusPoints", 0)), Is.True);
            Assert.That(attack.Perform(_characterSheet, ("targetDefence", 12)), Is.True);
            Assert.That(_roller.Rolled, Is.EqualTo(0));

            Assert.That(attack.Outcome(_characterSheet), Is.True);

            var roll = _characterSheet.GetRoll(attack.Id, "diceRoll");
            Assert.That(roll!.Result, Is.EqualTo(10));
            Assert.That(roll.Dice, Is.EqualTo(new[] { 4, 6 }));
            Assert.That(roll.SuppliedBy, Is.EqualTo(RpgRollSource.App));
            Assert.That(_characterSheet.GetPropertyValue<Dice>(attack.Id, "diceRoll").ToString(), Is.EqualTo("11"));

            attack.Complete(_characterSheet);

            //The roll is the same however often it is looked at, and after the turn has moved on
            _characterSheet.Time.Refresh();
            Assert.That(_characterSheet.GetPropertyValue<Dice>(attack.Id, "diceRoll").ToString(), Is.EqualTo("11"));

            _characterSheet.NextTurn();
            Assert.That(_characterSheet.GetRoll(attack.Id, "diceRoll")!.Result, Is.EqualTo(10));
            Assert.That(_roller.Rolled, Is.EqualTo(2));

            //The weapon's damage is still dice. It is for whoever is hit to roll.
            Assert.That(_sword.Damage.ToString(), Is.EqualTo("1d6"));
        }

        [Test]
        public void MeleeAttack_AutoComplete_AppRollsAndTheRollCanBeSeen()
        {
            UseDice(3, 3);
            _characterSheet.BeginTurnTracking();

            var attack = Start(_sword.Id, nameof(MeleeAttack));
            attack.AutoComplete(_characterSheet, ("actionPoints", 1), ("focusPoints", 0), ("targetDefence", 12));

            Assert.That(attack.IsComplete, Is.True);
            Assert.That(_pc.CurrentActionPoints, Is.EqualTo(0));

            var roll = _characterSheet.GetRoll(attack.Id, "diceRoll");
            Assert.That(roll!.Result, Is.EqualTo(6));
            Assert.That(roll.SuppliedBy, Is.EqualTo(RpgRollSource.App));
            Assert.That(_characterSheet.GetPropertyValue<Dice>(attack.Id, "diceRoll").ToString(), Is.EqualTo("7"));
        }

        [Test]
        public void MeleeAttack_RollReplacedByHand()
        {
            UseDice(1, 1);
            _characterSheet.RollMode = RpgRollMode.App;
            _characterSheet.BeginTurnTracking();

            var attack = Start(_sword.Id, nameof(MeleeAttack));
            Assert.That(attack.Cost(_characterSheet, ("actionPoints", 1), ("focusPoints", 0)), Is.True);
            Assert.That(attack.Perform(_characterSheet, ("targetDefence", 12)), Is.True);
            Assert.That(attack.Outcome(_characterSheet), Is.True);
            Assert.That(_characterSheet.GetPropertyValue<Dice>(attack.Id, "diceRoll").ToString(), Is.EqualTo("3"));

            //The GM allows the roll to be made again with real dice
            _characterSheet.SetRoll(attack, "diceRoll", 9);

            Assert.That(_characterSheet.GetPropertyValue<Dice>(attack.Id, "diceRoll").ToString(), Is.EqualTo("10"));
            Assert.That(_characterSheet.GetRoll(attack.Id, "diceRoll")!.SuppliedBy, Is.EqualTo(RpgRollSource.Player));
        }

        [Test]
        public void MeleeParry_AppDice_DamageIsReduced()
        {
            UseDice(5, 4);
            _characterSheet.RollMode = RpgRollMode.App;
            _characterSheet.BeginTurnTracking();

            var parry = Start(_pc.Id, nameof(MeleeParry));

            Assert.That(parry.Cost(_characterSheet, ("focusPoints", 0), ("damage", 10)), Is.True);
            Assert.That(parry.Perform(_characterSheet, ("parryTarget", 8)), Is.True);
            Assert.That(_characterSheet.GetPropertyValue<Dice>(parry.Id, "diceRoll").ToString(), Is.EqualTo("2d6 - 1"));

            Assert.That(parry.Outcome(_characterSheet), Is.True);

            //9 on the dice, less 1 for strength, meets the target of 8
            Assert.That(_characterSheet.GetRoll(parry.Id, "diceRoll")!.Result, Is.EqualTo(9));
            Assert.That(_characterSheet.GetPropertyValue<Dice>(parry.Id, "diceRoll").ToString(), Is.EqualTo("8"));
            Assert.That(_characterSheet.GetPropertyValue<Dice>(parry.Id, "damage").Number, Is.EqualTo(9));
        }

        [Test]
        public void ArmourCheck_OneRealDie_OneAppDie()
        {
            UseDice(2);
            _characterSheet.Move(_pc.Id, nameof(Actor.Wearing), _vest.Id);
            _characterSheet.BeginTurnTracking();

            var armourCheck = Start(_pc.Id, nameof(ArmourCheck));

            Assert.That(armourCheck.Cost(_characterSheet), Is.True);
            Assert.That(armourCheck.Perform(_characterSheet, ("luckPoints", 0)), Is.True);
            Assert.That(armourCheck.Outcome(_characterSheet, ("damage", 10)), Is.False);

            Assert.That(armourCheck.GetPendingRolls(_characterSheet).Select(x => x.Prop),
                Is.EquivalentTo(new[] { "diceRoll1", "diceRoll2" }));

            //The player rolls the first die by hand...
            _characterSheet.SetRoll(armourCheck, "diceRoll1", 5);
            Assert.That(armourCheck.Outcome(_characterSheet), Is.False);
            Assert.That(armourCheck.GetPendingRolls(_characterSheet).Select(x => x.Prop), Is.EqualTo(new[] { "diceRoll2" }));

            //...and lets the app roll the second
            var rolls = armourCheck.RollPending(_characterSheet);
            Assert.That(rolls.Length, Is.EqualTo(1));
            Assert.That(rolls[0].Result, Is.EqualTo(2));

            Assert.That(armourCheck.Outcome(_characterSheet), Is.True);

            //One success halves the damage
            Assert.That(_characterSheet.GetPropertyValue<Dice>(armourCheck.Id, "damage").Number, Is.EqualTo(5));
            Assert.That(_roller.Rolled, Is.EqualTo(1));
        }

        [Test]
        public void ArmourCheck_Luck_ReplacesADie_SoThereIsLessToRoll()
        {
            UseDice();
            _characterSheet.Move(_pc.Id, nameof(Actor.Wearing), _vest.Id);
            _characterSheet.BeginTurnTracking();

            var armourCheck = Start(_pc.Id, nameof(ArmourCheck));

            Assert.That(armourCheck.Cost(_characterSheet), Is.True);
            Assert.That(armourCheck.Perform(_characterSheet, ("luckPoints", 1)), Is.True);

            //Luck makes the first die a success without rolling it
            Assert.That(_characterSheet.GetPropertyValue<Dice>(armourCheck.Id, "diceRoll1").Number, Is.EqualTo(4));
            Assert.That(armourCheck.GetPendingRolls(_characterSheet).Select(x => x.Prop), Is.EqualTo(new[] { "diceRoll2" }));
        }

        [Test]
        public void TakeDamage_DamageGivenAsDice_NeedsARoll()
        {
            UseDice();
            _characterSheet.BeginTurnTracking();

            var takeDamage = Start(_pc.Id, nameof(TakeDamage));

            //The attacker's player reads out "1d6 + 2" rather than a number
            Assert.That(takeDamage.Outcome(_characterSheet, ("damage", "1d6 + 2")), Is.False);

            var pending = takeDamage.GetPendingRolls(_characterSheet);
            Assert.That(pending.Length, Is.EqualTo(1));
            Assert.That(pending[0].Prop, Is.EqualTo("damage"));
            Assert.That(pending[0].Expression.ToString(), Is.EqualTo("1d6 + 2"));
            Assert.That(_pc.CurrentStaminaPoints, Is.EqualTo(14));

            //Supplying a number for it is the result of the die
            Assert.That(takeDamage.Outcome(_characterSheet, ("damage", 4)), Is.True);
            takeDamage.Complete(_characterSheet);

            Assert.That(_pc.CurrentStaminaPoints, Is.EqualTo(8));
            Assert.That(_roller.Rolled, Is.EqualTo(0));
        }

        [Test]
        public void PendingRoll_SurvivesSaveAndRestore()
        {
            _characterSheet.BeginTurnTracking();

            var attack = Start(_sword.Id, nameof(MeleeAttack));
            Assert.That(attack.Cost(_characterSheet, ("actionPoints", 1), ("focusPoints", 0)), Is.True);
            Assert.That(attack.Perform(_characterSheet, ("targetDefence", 12)), Is.True);

            var characterSheet2 = TestSystem.RoundTrip(_characterSheet);
            var attack2 = (characterSheet2.GetObject(attack.Id) as RpgActivityAction)!;

            Assert.That(characterSheet2.GetPendingRolls().Length, Is.EqualTo(1));
            Assert.That(attack2.Outcome(characterSheet2), Is.False);

            characterSheet2.SetRoll(attack2, "diceRoll", 7);
            characterSheet2 = TestSystem.RoundTrip(characterSheet2);
            attack2 = (characterSheet2.GetObject(attack.Id) as RpgActivityAction)!;

            Assert.That(characterSheet2.GetRoll(attack2.Id, "diceRoll")!.Result, Is.EqualTo(7));
            Assert.That(attack2.Outcome(characterSheet2), Is.True);
            Assert.That(characterSheet2.GetPropertyValue<Dice>(attack2.Id, "diceRoll").ToString(), Is.EqualTo("8"));
        }
    }
}
