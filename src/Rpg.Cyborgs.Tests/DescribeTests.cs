using Rpg.Cyborgs.Actions;
using Rpg.Cyborgs.States;
using Rpg.Cyborgs.Tests.Models;
using Rpg.Experimental;
using Rpg.Experimental.Description;
using Rpg.Experimental.Json;
using Rpg.Experimental.Mods;

namespace Rpg.Cyborgs.Tests
{
    /// <summary>
    /// Asking the Cyborgs character sheet why things are as they are
    /// </summary>
    public class DescribeTests
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

        private RpgActivityAction Start(string ownerId, string actionName)
            => _characterSheet.CreateActivity(_pc.Id, ownerId, actionName).CurrentActivityAction!;

        private RpgActivityAction Continue(RpgActionRef[] nextActions, string actionName)
            => _characterSheet.CreateActivity(_pc.Id, nextActions.First(x => x.ActionName == actionName)).CurrentActivityAction!;

        [Test]
        public void Benny_DescribeDefence_EnsureValues()
        {
            var description = _characterSheet.Describe(_pc, x => x.Defence);

            Assert.That(description, Is.Not.Null);
            Assert.That(description!.Value.ToString(), Is.EqualTo("7"));
            Assert.That(description.ObjectName, Is.EqualTo("Benny"));
            Assert.That(description.Archetype, Is.EqualTo(nameof(PlayerCharacter)));
            Assert.That(description.Prop, Is.EqualTo("Defence"));

            Assert.That(description.Mods.Count, Is.EqualTo(2));
            Assert.That(description.Mods.Count(x => x.Kind == nameof(Initial)), Is.EqualTo(1));
            Assert.That(description.Mods.Count(x => x.Kind == nameof(Base)), Is.EqualTo(1));

            var starting = description.Mods.Single(x => x.Kind == nameof(Initial));
            Assert.That(starting.Contribution, Is.EqualTo(new Dice(7)));
            Assert.That(starting.Origin.Kind, Is.EqualTo(RpgOriginKind.StartingValue));

            var fromAgility = description.Mods.Single(x => x.Kind == nameof(Base));
            Assert.That(fromAgility.Origin.Kind, Is.EqualTo(RpgOriginKind.Derived));
            Assert.That(fromAgility.SourceProp, Is.EqualTo(nameof(Actor.Agility)));
            Assert.That(fromAgility.SourceProperty!.Value, Is.EqualTo(Dice.Zero));
        }

        [Test]
        public void Benny_DescribeFocusPoints_EnsureValues()
        {
            var description = _characterSheet.Describe(_pc, x => x.FocusPoints)!;

            Assert.That(description.Value.ToString(), Is.EqualTo("1"));
            Assert.That(description.Mods.Count, Is.EqualTo(3));
            Assert.That(description.Mods.All(x => x.Kind == nameof(Base) && x.Counts), Is.True);
            Assert.That(description.Mods.Select(x => x.SourceProp),
                Is.EquivalentTo(new[] { nameof(Actor.Agility), nameof(Actor.Brains), nameof(Actor.Insight) }));

            var fromBrains = description.Mods.Single(x => x.SourceProp == nameof(Actor.Brains));
            Assert.That(fromBrains.Contribution, Is.EqualTo(new Dice(1)));
            Assert.That(fromBrains.SourceProperty!.Mods.Single().Origin.Kind, Is.EqualTo(RpgOriginKind.StartingValue));
        }

        [Test]
        public void Benny_DescribeStamina_ShowsTheCalculation()
        {
            var description = _characterSheet.Describe(_pc, x => x.CurrentStaminaPoints)!;

            Assert.That(description.Value, Is.EqualTo(new Dice(14)));

            //Current stamina comes from stamina, which is a starting value plus twice health
            var stamina = description.Mods.Single().SourceProperty!;
            Assert.That(stamina.Prop, Is.EqualTo(nameof(Actor.StaminaPoints)));

            var fromHealth = stamina.Mods.Single(x => x.SourceProp == nameof(Actor.Health));
            Assert.That(fromHealth.Contribution, Is.EqualTo(new Dice(2)));
            Assert.That(fromHealth.Calculation!.Description, Is.EqualTo("Twice health"));
            Assert.That(fromHealth.Calculation.Input, Is.EqualTo(new Dice(1)));
            Assert.That(fromHealth.Calculation.Output, Is.EqualTo(new Dice(2)));

            var expected = string.Join(Environment.NewLine,
                "Benny.CurrentStaminaPoints = 14",
                "  +14 Base: From Benny.StaminaPoints",
                "      Benny.StaminaPoints = 14",
                "        +12 Initial: Starting value",
                "        +2 Base: From Benny.Health",
                "            calculation: Twice health (1 gives 2)",
                "            Benny.Health = 1",
                "              +1 Initial: Starting value");

            Assert.That(description.ToText(), Is.EqualTo(expected));
        }

        [Test]
        public void Benny_Summary()
        {
            var description = _characterSheet.DescribeObject(_pc);

            Assert.That(description.Name, Is.EqualTo("Benny"));
            Assert.That(description.Properties.Single(x => x.Prop == nameof(Actor.Defence)).Value, Is.EqualTo(new Dice(7)));
            Assert.That(description.Properties.Single(x => x.Prop == nameof(Actor.StaminaPoints)).Value, Is.EqualTo(new Dice(14)));
            Assert.That(description.Properties.Any(x => x.IsRollPending), Is.False);
            Assert.That(description.StatesOn, Is.Empty);
            Assert.That(description.StatesOff, Does.Contain(nameof(VeryFast)));
        }

        [Test]
        public void VeryFast_Off_SaysWhatItWouldChange()
        {
            var description = _characterSheet.DescribeState(_pc, nameof(VeryFast))!;

            Assert.That(description.IsOn, Is.False);
            Assert.That(description.Reason, Is.EqualTo(RpgStateReason.ConditionNotMet));
            Assert.That(description.Changes.Count, Is.EqualTo(1));
            Assert.That(description.Changes[0].Prop, Is.EqualTo(nameof(Actor.ActionPoints)));
            Assert.That(description.Changes[0].Change, Is.EqualTo(new Dice(1)));

            //The state shows on the stat it would change, as not counted
            var actionPoints = _characterSheet.Describe(_pc, x => x.ActionPoints)!;
            var fromState = actionPoints.Mods.Single(x => x.Origin.Kind == RpgOriginKind.State);

            Assert.That(actionPoints.Value, Is.EqualTo(new Dice(1)));
            Assert.That(fromState.Origin.Name, Is.EqualTo(nameof(VeryFast)));
            Assert.That(fromState.Counts, Is.False);
            Assert.That(fromState.WhyNot, Is.EqualTo("VeryFast is off"));
        }

        [Test]
        public void MeleeAttack_RollInProgress_IsExplained()
        {
            _characterSheet.BeginTurnTracking();

            var attack = Start(_sword.Id, nameof(MeleeAttack));
            Assert.That(attack.Cost(_characterSheet, ("actionPoints", 1), ("focusPoints", 1)), Is.True);
            Assert.That(attack.Perform(_characterSheet, ("targetDefence", 12)), Is.True);

            var roll = _characterSheet.Describe(attack, "diceRoll")!;

            Assert.That(roll.Value.ToString(), Is.EqualTo("2d6 + 1"));
            Assert.That(roll.IsRollPending, Is.True);
            Assert.That(roll.Mods.Select(x => x.Name ?? x.Kind),
                Is.EqualTo(new[] { nameof(Initial), "Ability and focus", "Weapon hit bonus" }));

            var hitBonus = roll.Mods.Single(x => x.Name == "Weapon hit bonus");
            Assert.That(hitBonus.Contribution, Is.EqualTo(new Dice(1)));
            Assert.That(hitBonus.Origin.Text, Is.EqualTo("From Excalibur.HitBonus"));
            Assert.That(hitBonus.SourceProperty!.ObjectName, Is.EqualTo("Excalibur"));

            var action = _characterSheet.DescribeAction(attack);

            Assert.That(action.ActionName, Is.EqualTo(nameof(MeleeAttack)));
            Assert.That(action.OwnerName, Is.EqualTo("Excalibur"));
            Assert.That(action.PendingRolls.Single().Prop, Is.EqualTo("diceRoll"));
            Assert.That(action.Costs.Select(x => x.Prop),
                Is.EquivalentTo(new[] { nameof(Actor.CurrentActionPoints), nameof(Actor.CurrentFocusPoints) }));

            //The player rolls 9
            Assert.That(attack.Outcome(_characterSheet, ("diceRoll", 9)), Is.True);
            attack.Complete(_characterSheet);

            roll = _characterSheet.Describe(attack, "diceRoll")!;
            Assert.That(roll.Value, Is.EqualTo(new Dice(10)));
            Assert.That(roll.ToText(), Does.StartWith("MeleeAttack.diceRoll = 10 (2d6 + 1: rolled 9 on 2d6 by the player)"));

            action = _characterSheet.DescribeAction(attack);
            Assert.That(action.IsComplete, Is.True);
            Assert.That(action.Rolls.Single().Roll.Result, Is.EqualTo(9));
            Assert.That(action.Effects.Single().IsStateActivation, Is.True);
            Assert.That(action.Effects.Single().ToString(), Is.EqualTo("Benny: state MeleeAttacking on, from turn 1 until turn 2"));

            //The damage is still the weapon's dice, and says so
            var damage = _characterSheet.Describe(attack, "damage")!;
            Assert.That(damage.Value.ToString(), Is.EqualTo("1d6"));
            Assert.That(damage.Mods.Single().Name, Is.EqualTo("Weapon damage"));

            //What the attack cost shows on the stats it was taken from
            var actionPoints = _characterSheet.Describe(_pc, x => x.CurrentActionPoints)!;
            var cost = actionPoints.Mods.Single(x => x.Origin.Kind == RpgOriginKind.ActionCost);

            Assert.That(actionPoints.Value, Is.EqualTo(Dice.Zero));
            Assert.That(cost.Origin.Text, Is.EqualTo("Cost of MeleeAttack"));
            Assert.That(cost.Lifespan, Is.EqualTo("from turn 1 until turn 2"));

            //And the state it switched on says why it is on
            var attacking = _characterSheet.DescribeState(_pc, nameof(MeleeAttacking))!;
            Assert.That(attacking.IsOn, Is.True);
            Assert.That(attacking.Reason, Is.EqualTo(RpgStateReason.Activated));
            Assert.That(attacking.ReasonText, Is.EqualTo("Switched on by effect of MeleeAttack (from turn 1 until turn 2)"));

            //Next turn the cost is shown as over
            _characterSheet.NextTurn();

            actionPoints = _characterSheet.Describe(_pc, x => x.CurrentActionPoints)!;
            cost = actionPoints.Mods.Single(x => x.Origin.Kind == RpgOriginKind.ActionCost);

            Assert.That(actionPoints.Value, Is.EqualTo(new Dice(1)));
            Assert.That(cost.Counts, Is.False);
            Assert.That(cost.WhyNot, Is.EqualTo("Ended at turn 2"));
        }

        [Test]
        public void Damage_ReducedByParryAndArmour_IsExplained()
        {
            _characterSheet.DiceRoller = new FixedDiceRoller(5, 4, 5, 2);
            _characterSheet.RollMode = RpgRollMode.App;
            _characterSheet.Move(_pc.Id, nameof(Actor.Wearing), _vest.Id);
            _characterSheet.BeginTurnTracking();

            //Parry: 9 on the dice less 1 for strength meets the target of 8
            var parry = Start(_pc.Id, nameof(MeleeParry));
            Assert.That(parry.Cost(_characterSheet, ("focusPoints", 0), ("damage", 10)), Is.True);
            Assert.That(parry.Perform(_characterSheet, ("parryTarget", 8)), Is.True);
            Assert.That(parry.Outcome(_characterSheet), Is.True);
            var nextActions = parry.Complete(_characterSheet);

            var parried = _characterSheet.Describe(parry, "damage")!;
            Assert.That(parried.Value, Is.EqualTo(new Dice(9)));
            Assert.That(parried.Mods.Single(x => x.Name == "Parried").Contribution, Is.EqualTo(new Dice(-1)));

            //Armour: one success out of two halves the damage
            var armourCheck = Continue(nextActions, nameof(ArmourCheck));
            Assert.That(armourCheck.Cost(_characterSheet), Is.True);
            Assert.That(armourCheck.Perform(_characterSheet, ("luckPoints", 0)), Is.True);
            Assert.That(armourCheck.Outcome(_characterSheet), Is.True);
            armourCheck.Complete(_characterSheet);

            var afterArmour = _characterSheet.Describe(armourCheck, "damage")!;

            var expected = string.Join(Environment.NewLine,
                "ArmourCheck.damage = 4",
                "  +9 Damage before armour: Set during ArmourCheck",
                "  -5 Halved by armour: Set during ArmourCheck");

            Assert.That(afterArmour.Value, Is.EqualTo(new Dice(4)));
            Assert.That(afterArmour.Mods.Where(x => x.Counts).Select(x => x.Name),
                Is.EqualTo(new[] { "Damage before armour", "Halved by armour" }));
            Assert.That(string.Join(Environment.NewLine, afterArmour.ToText().Split(Environment.NewLine).Where(x => !x.Contains("not counted"))),
                Is.EqualTo(expected));

            var armourAction = _characterSheet.DescribeAction(armourCheck);
            Assert.That(armourAction.Rolls.Select(x => x.Roll.Result), Is.EqualTo(new[] { 5, 2 }));
            Assert.That(armourAction.Effects.Single().ObjectName, Is.EqualTo("Vest"));
            Assert.That(armourAction.Effects.Single().Change, Is.EqualTo(new Dice(-1)));

            //The armour says what damaged it
            var armourRating = _characterSheet.Describe(_vest, x => x.CurrentArmourRating)!;
            Assert.That(armourRating.Value, Is.EqualTo(new Dice(2)));
            Assert.That(armourRating.Mods.Single(x => x.Origin.Kind == RpgOriginKind.ActionEffect).Origin.Text, Is.EqualTo("Effect of ArmourCheck"));

            //Take the damage that is left
            var takeDamage = Continue(nextActions, nameof(TakeDamage));
            Assert.That(takeDamage.Outcome(_characterSheet), Is.True);
            takeDamage.Complete(_characterSheet);

            var stamina = _characterSheet.Describe(_pc, x => x.CurrentStaminaPoints)!;
            var lost = stamina.Mods.Single(x => x.Origin.Kind == RpgOriginKind.ActionEffect);

            Assert.That(stamina.Value, Is.EqualTo(new Dice(10)));
            Assert.That(lost.Contribution, Is.EqualTo(new Dice(-4)));
            Assert.That(lost.Origin.Text, Is.EqualTo("Effect of TakeDamage"));
        }

        [Test]
        public void Damage_StoppedByArmour_NoneIsCarriedForward()
        {
            _characterSheet.DiceRoller = new FixedDiceRoller(5, 4, 5, 5);
            _characterSheet.RollMode = RpgRollMode.App;
            _characterSheet.Move(_pc.Id, nameof(Actor.Wearing), _vest.Id);
            _characterSheet.BeginTurnTracking();

            var parry = Start(_pc.Id, nameof(MeleeParry));
            parry.Cost(_characterSheet, ("focusPoints", 0), ("damage", 10));
            parry.Perform(_characterSheet, ("parryTarget", 8));
            parry.Outcome(_characterSheet);
            var nextActions = parry.Complete(_characterSheet);

            //Both armour dice succeed, so the armour stops everything
            var armourCheck = Continue(nextActions, nameof(ArmourCheck));
            armourCheck.Cost(_characterSheet);
            armourCheck.Perform(_characterSheet, ("luckPoints", 0));
            Assert.That(armourCheck.Outcome(_characterSheet), Is.True);
            armourCheck.Complete(_characterSheet);

            var stopped = _characterSheet.Describe(armourCheck, "damage")!;
            Assert.That(stopped.Value, Is.EqualTo(Dice.Zero));
            Assert.That(stopped.Mods.Where(x => x.Counts).Select(x => x.Name),
                Is.EqualTo(new[] { "Damage before armour", "Stopped by armour" }));

            //Taking damage afterwards takes none, not the 9 from before the armour check
            var takeDamage = Continue(nextActions, nameof(TakeDamage));
            Assert.That(_characterSheet.GetPropertyValue<Dice>(takeDamage.Id, "damage"), Is.EqualTo(Dice.Zero));
            Assert.That(takeDamage.Outcome(_characterSheet), Is.True);
            takeDamage.Complete(_characterSheet);

            Assert.That(_pc.CurrentStaminaPoints, Is.EqualTo(14));
        }

        [Test]
        public void ChangeByHand_ShowsOnDerivedStats()
        {
            //The GM rules that Benny is stronger than his sheet says
            _characterSheet.OverrideByHand(_pc, x => x.Strength, 2);

            Assert.That(_pc.Strength, Is.EqualTo(2));
            Assert.That(_pc.LifePoints, Is.EqualTo(8));

            var lifePoints = _characterSheet.Describe(_pc, x => x.LifePoints)!;
            var strength = lifePoints.Mods.Single(x => x.SourceProp == nameof(Actor.Strength)).SourceProperty!;

            Assert.That(strength.Mods.Single(x => x.Counts).Origin.Kind, Is.EqualTo(RpgOriginKind.Manual));
            Assert.That(strength.Mods.Single(x => !x.Counts).WhyNot, Is.EqualTo("Set aside by an override"));
            Assert.That(_characterSheet.GetManualChanges().Length, Is.EqualTo(1));
        }

        [Test]
        public void Describe_NeverChangesTheSheet()
        {
            var roller = new FixedDiceRoller();
            _characterSheet.DiceRoller = roller;
            _characterSheet.BeginTurnTracking();

            var attack = Start(_sword.Id, nameof(MeleeAttack));
            attack.Cost(_characterSheet, ("actionPoints", 1), ("focusPoints", 0));
            attack.Perform(_characterSheet, ("targetDefence", 12));

            var before = RpgJson.SerializeGraphState(_characterSheet.GetState());

            foreach (var property in _characterSheet.DescribeObject(_pc).Properties)
                Assert.That(_characterSheet.Describe(_pc, property.Prop), Is.Not.Null);

            foreach (var state in _characterSheet.GetObjectStates(_pc.Id))
                Assert.That(_characterSheet.DescribeModSet(state).ToText(), Is.Not.Empty);

            Assert.That(_characterSheet.DescribeAction(attack).ToText(), Is.Not.Empty);
            Assert.That(_characterSheet.Describe(attack, "diceRoll")!.ToText(), Is.Not.Empty);
            Assert.That(_characterSheet.DescribeObject(_sword).ToText(), Is.Not.Empty);

            Assert.That(RpgJson.SerializeGraphState(_characterSheet.GetState()), Is.EqualTo(before));
            Assert.That(roller.Rolled, Is.EqualTo(0));
        }
    }
}
