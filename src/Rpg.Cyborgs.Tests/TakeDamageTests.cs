using Rpg.Cyborgs.Actions;
using Rpg.Cyborgs.Conditions;
using Rpg.Cyborgs.States;
using Rpg.Cyborgs.Tests.Models;
using Rpg.Experimental;
using Rpg.Experimental.Reflection.Args;

namespace Rpg.Cyborgs.Tests
{
    public class TakeDamageTests
    {
        private RpgCharacterSheet _characterSheet;
        private PlayerCharacter _pc;
        private MeleeWeapon _sword;
        private Armour _vest;

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

        [Test]
        public void TakeDamage_10Damage_EnsureValues()
        {
            Assert.That(_pc.CurrentStaminaPoints, Is.EqualTo(14));

            _characterSheet.Time.BeginEncounter();

            var activity = _characterSheet.CreateActivity(_pc.Id, _pc.Id, nameof(TakeDamage));
            Assert.That(activity.Expiry, Is.EqualTo(LifecycleExpiry.Active));

            var takeDamage = activity.CurrentActivityAction;
            Assert.That(takeDamage, Is.Not.Null);
            Assert.That(takeDamage.IsComplete, Is.False);

            //TakeDamage has no cost or perform steps
            Assert.That(takeDamage.CostMethod.Args.Count(), Is.EqualTo(0));
            Assert.That(takeDamage.Cost(_characterSheet), Is.True);
            Assert.That(takeDamage.PerformMethod.Args.Count(), Is.EqualTo(0));
            Assert.That(takeDamage.Perform(_characterSheet), Is.True);

            Assert.That(takeDamage.OutcomeMethod.Args.Count(), Is.EqualTo(4));
            Assert.That(takeDamage.OutcomeMethod.Args.IsComplete(), Is.False);
            Assert.That(takeDamage.Outcome(_characterSheet), Is.False);

            Assert.That(takeDamage.Outcome(_characterSheet, ("damage", 10)), Is.True);
            Assert.That(takeDamage.Result.Mods.Count(), Is.EqualTo(1));
            Assert.That(takeDamage.AllStepsComplete, Is.True);

            //Nothing happens until the action is completed
            Assert.That(_pc.CurrentStaminaPoints, Is.EqualTo(14));

            var nextActions = takeDamage.Complete();
            _characterSheet.Time.Refresh();

            Assert.That(takeDamage.IsComplete, Is.True);
            Assert.That(nextActions.Length, Is.EqualTo(0));
            Assert.That(_pc.CurrentStaminaPoints, Is.EqualTo(4));
            Assert.That(_pc.CurrentLifePoints, Is.EqualTo(5));

            //The damage outlives the activity
            _characterSheet.Time.ToTurn(2);

            Assert.That(activity.Expiry, Is.EqualTo(LifecycleExpiry.Expired));
            Assert.That(takeDamage.Expiry, Is.EqualTo(LifecycleExpiry.Expired));
            Assert.That(_pc.CurrentStaminaPoints, Is.EqualTo(4));

            _characterSheet.Time.EndEncounter();

            Assert.That(_characterSheet.ObjectExists(activity.Id), Is.False);
            Assert.That(_characterSheet.ObjectExists(takeDamage.Id), Is.False);
            Assert.That(_pc.CurrentStaminaPoints, Is.EqualTo(4));
        }

        [Test]
        public void TakeDamage_TwiceOverTwoTurns_DamageAccumulates()
        {
            _characterSheet.Time.BeginEncounter();

            var first = _characterSheet.CreateActivity(_pc.Id, _pc.Id, nameof(TakeDamage)).CurrentActivityAction!;
            Assert.That(first.Outcome(_characterSheet, ("damage", 3)), Is.True);
            first.Complete();
            _characterSheet.Time.Refresh();

            Assert.That(_pc.CurrentStaminaPoints, Is.EqualTo(11));

            _characterSheet.Time.ToTurn(2);

            var second = _characterSheet.CreateActivity(_pc.Id, _pc.Id, nameof(TakeDamage)).CurrentActivityAction!;
            Assert.That(second.Id, Is.Not.EqualTo(first.Id));
            Assert.That(second.Outcome(_characterSheet, ("damage", 4)), Is.True);
            second.Complete();
            _characterSheet.Time.Refresh();

            Assert.That(_pc.CurrentStaminaPoints, Is.EqualTo(7));

            _characterSheet.Time.EndEncounter();

            Assert.That(_pc.CurrentStaminaPoints, Is.EqualTo(7));
        }

        [Test]
        public void TakeDamage_15Damage_NextAction_TakeInjury()
        {
            Assert.That(_pc.CurrentStaminaPoints, Is.EqualTo(14));
            Assert.That(_pc.CurrentLifePoints, Is.EqualTo(5));

            _characterSheet.Time.BeginEncounter();

            var activity = _characterSheet.CreateActivity(_pc.Id, _pc.Id, nameof(TakeDamage));
            var takeDamage = activity.CurrentActivityAction!;

            Assert.That(takeDamage.Outcome(_characterSheet, ("damage", 15)), Is.True);
            Assert.That(takeDamage.Result.Mods.Count(), Is.EqualTo(2));
            Assert.That(_characterSheet.GetPropertyValue<Dice>(takeDamage.Id, "staminaInjury").ToString(), Is.EqualTo("14"));
            Assert.That(_characterSheet.GetPropertyValue<Dice>(takeDamage.Id, "lifeInjury").ToString(), Is.EqualTo("1"));

            var nextActions = takeDamage.Complete();
            _characterSheet.Time.Refresh();

            Assert.That(nextActions.Length, Is.EqualTo(1));
            Assert.That(nextActions[0].ActionName, Is.EqualTo(nameof(TakeInjury)));
            Assert.That(nextActions[0].ActionOwnerId, Is.EqualTo(_pc.Id));
            Assert.That(nextActions[0].Optional, Is.False);

            Assert.That(_pc.CurrentStaminaPoints, Is.EqualTo(0));
            Assert.That(_pc.CurrentLifePoints, Is.EqualTo(4));
            Assert.That(_pc.IsStateOn(nameof(Exhausted)), Is.True);

            //The injury action continues the same activity and picks up the life injury from the damage action
            var activity2 = _characterSheet.CreateActivity(_pc.Id, nextActions[0]);
            Assert.That(activity2.Id, Is.EqualTo(activity.Id));
            Assert.That(activity.ActivityActions.Count, Is.EqualTo(2));

            var takeInjury = activity.CurrentActivityAction!;
            Assert.That(takeInjury.Id, Is.Not.EqualTo(takeDamage.Id));
            Assert.That(takeInjury.IsComplete, Is.False);
            Assert.That(takeInjury.PerformMethod.Args.IsComplete(), Is.True);
            Assert.That(takeInjury.PerformMethod.Args.Find("lifeInjury")?.Value, Is.EqualTo(1));

            Assert.That(takeInjury.Cost(_characterSheet), Is.True);
            Assert.That(takeInjury.Perform(_characterSheet), Is.True);
            Assert.That(_characterSheet.GetPropertyValue<Dice>(takeInjury.Id, "injuryRoll").ToString(), Is.EqualTo("2d6 - 1"));
            Assert.That(_characterSheet.GetPropertyValue<Dice>(takeInjury.Id, "injuryLocationRoll").ToString(), Is.EqualTo("1d6"));

            var injuryOutcome = takeInjury.Outcome(_characterSheet,
                ("injuryRoll", 3),
                ("injuryLocationRoll", 3), //Left arm
                ("locationType", 0)); //Random location

            Assert.That(injuryOutcome, Is.True);
            Assert.That(takeInjury.AllStepsComplete, Is.True);
            Assert.That(_pc.LeftArm.InjurySeverity, Is.EqualTo(0));

            takeInjury.Complete();
            _characterSheet.Time.Refresh();

            Assert.That(_pc.LeftArm.InjurySeverity, Is.EqualTo((int)InjurySeverityEnum.Severed));
            Assert.That(_pc.LeftArm.Injuries.Count(), Is.EqualTo(1));
            Assert.That(_pc.LeftArm.Injuries[0].Severity, Is.EqualTo(5));

            _characterSheet.Time.ToTurn(2);

            Assert.That(activity.Expiry, Is.EqualTo(LifecycleExpiry.Expired));
            Assert.That(takeInjury.Expiry, Is.EqualTo(LifecycleExpiry.Expired));

            //The damage and the injury outlive the activity
            Assert.That(_pc.CurrentStaminaPoints, Is.EqualTo(0));
            Assert.That(_pc.CurrentLifePoints, Is.EqualTo(4));
            Assert.That(_pc.LeftArm.InjurySeverity, Is.EqualTo(5));
            Assert.That(_pc.LeftArm.Injuries.Count(), Is.EqualTo(1));
            Assert.That(_pc.RightArm.InjurySeverity, Is.EqualTo(0));

            //...and switch on the conditions that depend on them
            Assert.That(_pc.LeftArm.IsStateOn(nameof(Attachable)), Is.True);
            Assert.That(_pc.LeftArm.IsStateOn(nameof(Bleeding)), Is.True);
            Assert.That(_pc.LeftArm.IsStateOn(nameof(Pain)), Is.False);
            Assert.That(_pc.RightArm.ActiveStates, Is.Empty);
            Assert.That(_pc.IsStateOn(nameof(Dying)), Is.True);
        }

        [Test]
        public void MeleeParry_Then_TakeDamage_DamageIsReduced()
        {
            _characterSheet.Time.BeginEncounter();

            var activity = _characterSheet.CreateActivity(_pc.Id, _pc.Id, nameof(MeleeParry));
            var parry = activity.CurrentActivityAction!;

            Assert.That(parry.Cost(_characterSheet, ("focusPoints", 0), ("damage", 10)), Is.True);
            Assert.That(parry.Perform(_characterSheet, ("parryTarget", 8)), Is.True);

            //2d6 + Strength
            Assert.That(_characterSheet.GetPropertyValue<Dice>(parry.Id, "diceRoll").ToString(), Is.EqualTo("2d6 - 1"));

            Assert.That(parry.Outcome(_characterSheet, ("diceRoll", 9)), Is.True);

            //A successful parry reduces damage by at least 1
            Assert.That(_characterSheet.GetPropertyValue<Dice>(parry.Id, "damage").Roll(), Is.EqualTo(9));

            var nextActions = parry.Complete();
            _characterSheet.Time.Refresh();

            Assert.That(nextActions.Length, Is.EqualTo(2));
            Assert.That(nextActions[0].ActionName, Is.EqualTo(nameof(ArmourCheck)));
            Assert.That(nextActions[0].Optional, Is.False);
            Assert.That(nextActions[1].ActionName, Is.EqualTo(nameof(TakeDamage)));
            Assert.That(nextActions[1].Optional, Is.True);

            //Benny wears no armour so go straight to taking the damage
            _characterSheet.CreateActivity(_pc.Id, nextActions[1]);
            var takeDamage = activity.CurrentActivityAction!;

            Assert.That(takeDamage.GetAction()?.Name, Is.EqualTo(nameof(TakeDamage)));
            Assert.That(_characterSheet.GetPropertyValue<Dice>(takeDamage.Id, "damage").Roll(), Is.EqualTo(9));
            Assert.That(takeDamage.Outcome(_characterSheet), Is.True);

            takeDamage.Complete();
            _characterSheet.Time.Refresh();

            Assert.That(_pc.CurrentStaminaPoints, Is.EqualTo(5));

            //Parrying costs next turn's action
            _characterSheet.Time.ToTurn(2);

            Assert.That(_pc.IsStateOn(nameof(Parrying)), Is.True);
            Assert.That(_pc.CurrentActionPoints, Is.EqualTo(0));
            Assert.That(_pc.CurrentStaminaPoints, Is.EqualTo(5));

            _characterSheet.Time.ToTurn(3);

            Assert.That(_pc.IsStateOn(nameof(Parrying)), Is.False);
            Assert.That(_pc.CurrentActionPoints, Is.EqualTo(1));
        }

        [Test]
        public void MeleeParry_ArmourCheck_TakeDamage_DamageIsReducedTwice()
        {
            _characterSheet.Move(_pc.Id, nameof(Actor.Wearing), _vest.Id);
            _characterSheet.Time.BeginEncounter();

            Assert.That(_pc.Wearing.Contains(_vest), Is.True);
            Assert.That(_vest.CurrentArmourRating, Is.EqualTo(3));

            var activity = _characterSheet.CreateActivity(_pc.Id, _pc.Id, nameof(MeleeParry));
            var parry = activity.CurrentActivityAction!;

            Assert.That(parry.Cost(_characterSheet, ("focusPoints", 0), ("damage", 10)), Is.True);
            Assert.That(parry.Perform(_characterSheet, ("parryTarget", 8)), Is.True);
            Assert.That(parry.Outcome(_characterSheet, ("diceRoll", 9)), Is.True);

            var parryNextActions = parry.Complete();
            _characterSheet.Time.Refresh();

            _characterSheet.CreateActivity(_pc.Id, parryNextActions.First(x => x.ActionName == nameof(ArmourCheck)));
            var armourCheck = activity.CurrentActivityAction!;

            Assert.That(armourCheck.GetAction()?.Name, Is.EqualTo(nameof(ArmourCheck)));
            Assert.That(_characterSheet.GetPropertyValue<Dice>(armourCheck.Id, "damage").Roll(), Is.EqualTo(9));

            Assert.That(armourCheck.Cost(_characterSheet), Is.True);
            Assert.That(armourCheck.Perform(_characterSheet, ("luckPoints", 0)), Is.True);
            Assert.That(_characterSheet.GetPropertyValue<Dice>(armourCheck.Id, "armourRating").Roll(), Is.EqualTo(3));
            Assert.That(_characterSheet.GetPropertyValue<Dice>(armourCheck.Id, "diceRoll1").ToString(), Is.EqualTo("1d6"));

            //One success halves the damage (rounded in the defender's favour) and damages the armour
            Assert.That(armourCheck.Outcome(_characterSheet, ("diceRoll1", 5), ("diceRoll2", 2)), Is.True);
            Assert.That(_characterSheet.GetPropertyValue<Dice>(armourCheck.Id, "damage").Roll(), Is.EqualTo(4));

            armourCheck.Complete();
            _characterSheet.Time.Refresh();

            Assert.That(_vest.CurrentArmourRating, Is.EqualTo(2));

            _characterSheet.CreateActivity(_pc.Id, parryNextActions.First(x => x.ActionName == nameof(TakeDamage)));
            var takeDamage = activity.CurrentActivityAction!;

            Assert.That(activity.ActivityActions.Count, Is.EqualTo(3));
            Assert.That(_characterSheet.GetPropertyValue<Dice>(takeDamage.Id, "damage").Roll(), Is.EqualTo(4));
            Assert.That(takeDamage.Outcome(_characterSheet), Is.True);

            takeDamage.Complete();
            _characterSheet.Time.Refresh();

            Assert.That(_pc.CurrentStaminaPoints, Is.EqualTo(10));

            //Armour damage is permanent
            _characterSheet.Time.EndEncounter();

            Assert.That(_vest.CurrentArmourRating, Is.EqualTo(2));
            Assert.That(_pc.CurrentStaminaPoints, Is.EqualTo(10));
        }
    }
}
