using Newtonsoft.Json;
using Rpg.Experimental.Description;
using Rpg.Experimental.Graph;
using Rpg.Experimental.Json;
using Rpg.Experimental.Mods;
using Rpg.Experimental.Reflection;
using Rpg.Experimental.Tests.Models;
using Temporal = Rpg.Experimental.Mods.Temporal;

namespace Rpg.Experimental.Tests
{
    /// <summary>
    /// Describe, as set out in feature 4 of docs/VISION.md: why a value is what it is, what a state or mod
    /// set does, and what an action has done or is waiting for.
    /// </summary>
    public class Describe_Tests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeUtilities.RegisterAssembly(typeof(TestObject).Assembly);
        }

        private static string State(RpgCharacterSheet characterSheet)
            => RpgJson.SerializeGraphState(characterSheet.GetState());

        private static RpgActivityAction Start(RpgCharacterSheet characterSheet, TestCreature creature, string actionName)
            => characterSheet.CreateActivity(creature.Id, creature.Id, actionName).CurrentActivityAction!;

        #region Properties

        [Test]
        public void Property_StartingValue()
        {
            var obj = new TestObject("Thing");
            var characterSheet = new RpgCharacterSheet(obj);

            var description = characterSheet.Describe(obj, x => x.Strength)!;

            Assert.That(description.ObjectId, Is.EqualTo(obj.Id));
            Assert.That(description.ObjectName, Is.EqualTo("Thing"));
            Assert.That(description.Archetype, Is.EqualTo(nameof(TestObject)));
            Assert.That(description.Prop, Is.EqualTo("Strength"));
            Assert.That(description.DisplayName, Is.EqualTo("Str"));
            Assert.That(description.Value, Is.EqualTo(new Dice(10)));
            Assert.That(description.Mods.Count, Is.EqualTo(1));

            var mod = description.Mods[0];
            Assert.That(mod.Kind, Is.EqualTo(nameof(Initial)));
            Assert.That(mod.Contribution, Is.EqualTo(new Dice(10)));
            Assert.That(mod.FixedValue, Is.EqualTo(new Dice(10)));
            Assert.That(mod.Counts, Is.True);
            Assert.That(mod.Status, Is.EqualTo(RpgModStatus.Counts));
            Assert.That(mod.Origin.Kind, Is.EqualTo(RpgOriginKind.StartingValue));
            Assert.That(mod.Lifespan, Is.EqualTo("permanent"));
            Assert.That(mod.SourceProperty, Is.Null);
        }

        [Test]
        public void Property_UnknownProperty_NoDescription()
        {
            var obj = new TestObject("Thing");
            var characterSheet = new RpgCharacterSheet(obj);

            Assert.That(characterSheet.Describe(obj, "NoSuchThing"), Is.Null);
            Assert.That(characterSheet.Describe(obj, "Child.Strength"), Is.Null);
        }

        [Test]
        public void Property_Derived_IsATreeDownToStartingValues()
        {
            var obj = new TestObject("Thing");
            var characterSheet = new RpgCharacterSheet(obj);

            var description = characterSheet.Describe(obj, x => x.Damage)!;

            Assert.That(description.Value.ToString(), Is.EqualTo("1d6 + 11"));
            Assert.That(description.Mods.Count, Is.EqualTo(2));

            var starting = description.Mods.Single(x => x.Origin.Kind == RpgOriginKind.StartingValue);
            Assert.That(starting.Contribution!.Value.ToString(), Is.EqualTo("1d6 + 1"));

            var derived = description.Mods.Single(x => x.Origin.Kind == RpgOriginKind.Derived);
            Assert.That(derived.Contribution, Is.EqualTo(new Dice(10)));
            Assert.That(derived.Origin.Text, Is.EqualTo("From Thing.Str"));
            Assert.That(derived.SourceObjectId, Is.EqualTo(obj.Id));
            Assert.That(derived.SourceProp, Is.EqualTo("Strength"));

            //The source is described the same way
            Assert.That(derived.SourceProperty, Is.Not.Null);
            Assert.That(derived.SourceProperty!.Prop, Is.EqualTo("Strength"));
            Assert.That(derived.SourceProperty.Value, Is.EqualTo(new Dice(10)));
            Assert.That(derived.SourceProperty.Mods.Single().Origin.Kind, Is.EqualTo(RpgOriginKind.StartingValue));
        }

        [Test]
        public void Property_Calculation_ShowsInputOutputAndDescription()
        {
            var obj = new TestObject("Thing");
            var characterSheet = new RpgCharacterSheet(obj);
            characterSheet.OverrideByHand(obj, x => x.Strength, 16);

            var description = characterSheet.Describe(obj, x => x.StrengthBonus)!;
            var mod = description.Mods.Single();

            Assert.That(description.Value, Is.EqualTo(new Dice(3)));
            Assert.That(mod.Calculation, Is.Not.Null);
            Assert.That(mod.Calculation!.Name, Is.EqualTo("CalculateBonus"));
            Assert.That(mod.Calculation.Description, Is.EqualTo("Half of the score above ten, rounded down"));
            Assert.That(mod.Calculation.Input, Is.EqualTo(new Dice(16)));
            Assert.That(mod.Calculation.Output, Is.EqualTo(new Dice(3)));
            Assert.That(mod.Calculation.ToText(), Is.EqualTo("Half of the score above ten, rounded down (16 gives 3)"));
        }

        [Test]
        public void Property_DepthOne_DoesNotFollowSources()
        {
            var obj = new TestObject("Thing");
            var characterSheet = new RpgCharacterSheet(obj);

            var description = characterSheet.Describe(obj, x => x.Damage, 1)!;
            var derived = description.Mods.Single(x => x.Origin.Kind == RpgOriginKind.Derived);

            //The source is named and valued, but its own mods are left for when it is asked about
            Assert.That(derived.SourceProperty, Is.Not.Null);
            Assert.That(derived.SourceProperty!.Value, Is.EqualTo(new Dice(10)));
            Assert.That(derived.SourceProperty.Mods, Is.Empty);
            Assert.That(derived.SourceProperty.IsTruncated, Is.True);
        }

        [Test]
        public void Property_SourceThroughChildObject_IsFollowed()
        {
            var obj = new TestObject("Thing");
            var child = new TestObject("Kid");
            obj.Child = child;

            var characterSheet = new RpgCharacterSheet(obj);
            characterSheet.Add(new Standard()
                .SetTarget(obj, x => x.Initiative)
                .SetSource(new RpgPropertyRef(obj.Id, "Child.Strength")));
            characterSheet.Time.Refresh();

            Assert.That(obj.Initiative, Is.EqualTo(new Dice(10)));

            var description = characterSheet.Describe(obj, x => x.Initiative)!;
            var derived = description.Mods.Single(x => x.Origin.Kind == RpgOriginKind.Derived);

            Assert.That(derived.SourceObjectId, Is.EqualTo(child.Id));
            Assert.That(derived.SourceProp, Is.EqualTo("Strength"));
            Assert.That(derived.Origin.Text, Is.EqualTo("From Kid.Str"));
            Assert.That(derived.SourceProperty!.ObjectName, Is.EqualTo("Kid"));

            //The child's property can also be asked for by path
            var byPath = characterSheet.Describe(obj, "Child.Strength")!;
            Assert.That(byPath.ObjectId, Is.EqualTo(child.Id));
            Assert.That(byPath.Value, Is.EqualTo(new Dice(10)));

            //A change to the child reaches the parent
            characterSheet.OverrideByHand(child, x => x.Strength, 14);
            Assert.That(obj.Initiative, Is.EqualTo(new Dice(14)));
        }

        [Test]
        public void Property_Loop_IsMarkedAndDoesNotHang()
        {
            var obj = new TestObject("Thing");
            var characterSheet = new RpgCharacterSheet(obj);

            //Two virtual properties that each take their value from the other
            characterSheet.CreateVirtualProperty(obj, "A", 1);
            characterSheet.CreateVirtualProperty(obj, "B", 1);

            var a = characterSheet.GetPropertyData<RpgPropertyDataModdable>(obj.Id, "A")!;
            var b = characterSheet.GetPropertyData<RpgPropertyDataModdable>(obj.Id, "B")!;

            //Two expired mods added behind the engine's back. Live ones would send the engine itself round the loop.
            var aFromB = new Standard().SetTarget(obj.Id, "A").SetSource(new RpgPropertyRef(obj.Id, "B"));
            var bFromA = new Standard().SetTarget(obj.Id, "B").SetSource(new RpgPropertyRef(obj.Id, "A"));
            characterSheet.BeginTurnTracking();
            aFromB.Expire(characterSheet, new Rpg.Experimental.Time.TimePoint(TimePointType.Turn, 1));
            bFromA.Expire(characterSheet, new Rpg.Experimental.Time.TimePoint(TimePointType.Turn, 1));
            a.Mods.Add(aFromB);
            b.Mods.Add(bFromA);

            var description = characterSheet.Describe(obj, "A")!;

            var fromB = description.Mods.Single(x => x.SourceProp == "B");
            var backToA = fromB.SourceProperty!.Mods.Single(x => x.SourceProp == "A");

            Assert.That(backToA.SourceProperty!.IsLoop, Is.True);
            Assert.That(backToA.SourceProperty.Mods, Is.Empty);
            Assert.That(description.ToText(), Does.Contain("already shown above"));
        }

        #endregion Properties

        #region Mods that do not count

        [Test]
        public void NotCounted_SetAsideByOverride()
        {
            var obj = new TestObject("Thing");
            var characterSheet = new RpgCharacterSheet(obj);
            characterSheet.OverrideByHand(obj, x => x.Strength, 16);

            var description = characterSheet.Describe(obj, x => x.Strength)!;

            Assert.That(description.Value, Is.EqualTo(new Dice(16)));
            Assert.That(description.Mods.Count, Is.EqualTo(2));

            var starting = description.Mods.Single(x => x.Kind == nameof(Initial));
            Assert.That(starting.Counts, Is.False);
            Assert.That(starting.Status, Is.EqualTo(RpgModStatus.Overridden));
            Assert.That(starting.WhyNot, Is.EqualTo("Set aside by an override"));
            Assert.That(starting.Contribution, Is.EqualTo(new Dice(10)));

            var over = description.Mods.Single(x => x.Kind == nameof(Override));
            Assert.That(over.Counts, Is.True);
        }

        [Test]
        public void NotCounted_Expired_ShownWithTheTurnItEnded()
        {
            var creature = new TestCreature("Rat");
            var characterSheet = new RpgCharacterSheet(creature);
            characterSheet.BeginTurnTracking();

            characterSheet.Add(new Temporal(2).SetName("Stimulant"), creature, x => x.Bonus, 2);
            characterSheet.Time.Refresh();

            var during = characterSheet.Describe(creature, x => x.Bonus)!;
            var stimulant = during.Mods.Single(x => x.Name == "Stimulant");
            Assert.That(during.Value, Is.EqualTo(new Dice(2)));
            Assert.That(stimulant.Counts, Is.True);
            Assert.That(stimulant.Lifespan, Is.EqualTo("from turn 1 until turn 3"));

            characterSheet.AdvanceToTurn(3);

            var after = characterSheet.Describe(creature, x => x.Bonus)!;
            stimulant = after.Mods.Single(x => x.Name == "Stimulant");
            Assert.That(after.Value, Is.EqualTo(Dice.Zero));
            Assert.That(stimulant.Counts, Is.False);
            Assert.That(stimulant.Status, Is.EqualTo(RpgModStatus.Expired));
            Assert.That(stimulant.WhyNot, Is.EqualTo("Ended at turn 3"));
            Assert.That(stimulant.Contribution, Is.EqualTo(new Dice(2)));
        }

        [Test]
        public void NotCounted_NotStartedYet()
        {
            var creature = new TestCreature("Rat");
            var characterSheet = new RpgCharacterSheet(creature);
            characterSheet.BeginTurnTracking();

            characterSheet.Add(new Temporal(2, 1), creature, x => x.Bonus, 2);
            characterSheet.Time.Refresh();

            var mod = characterSheet.Describe(creature, x => x.Bonus)!.Mods.Single(x => x.Kind == nameof(Temporal));

            Assert.That(mod.Counts, Is.False);
            Assert.That(mod.Status, Is.EqualTo(RpgModStatus.NotStarted));
            Assert.That(mod.WhyNot, Is.EqualTo("Starts at turn 3"));
            Assert.That(mod.Lifespan, Is.EqualTo("from turn 3 until turn 4"));
        }

        [Test]
        public void NotCounted_UntilATimeEvent()
        {
            var creature = new TestCreature("Rat");
            var characterSheet = new RpgCharacterSheet(creature);

            characterSheet.Add(new Standard().Until("Sunrise"), creature, x => x.Bonus, 2);
            characterSheet.Time.Refresh();

            var mod = characterSheet.Describe(creature, x => x.Bonus)!.Mods.Single(x => x.Kind == nameof(Standard));

            Assert.That(mod.Counts, Is.True);
            Assert.That(mod.Lifespan, Is.EqualTo("until Sunrise"));
        }

        [Test]
        public void NotCounted_StateIsOff()
        {
            var obj = new TestObject("Thing");
            var characterSheet = new RpgCharacterSheet(obj);

            var mod = characterSheet.Describe(obj, x => x.Initiative)!.Mods.Single();

            Assert.That(mod.Counts, Is.False);
            Assert.That(mod.Status, Is.EqualTo(RpgModStatus.NotApplied));
            Assert.That(mod.WhyNot, Is.EqualTo("TestState is off"));
            Assert.That(mod.Origin.Kind, Is.EqualTo(RpgOriginKind.State));
            Assert.That(mod.Origin.Name, Is.EqualTo(nameof(TestState)));
            Assert.That(mod.Origin.Text, Is.EqualTo("State TestState on Thing"));
        }

        #endregion Mods that do not count

        #region Changes made by hand

        [Test]
        public void ByHand_ShowsAsManual_AndCanBeListedAndUndone()
        {
            var obj = new TestObject("Thing");
            var characterSheet = new RpgCharacterSheet(obj);

            Assert.That(characterSheet.GetManualChanges(), Is.Empty);

            var over = characterSheet.OverrideByHand(obj, x => x.Strength, 16);
            var adjust = characterSheet.AdjustByHand(obj, x => x.Intelligence, 2);

            Assert.That(obj.Strength, Is.EqualTo(16));
            Assert.That(obj.Intelligence, Is.EqualTo(5));

            var strength = characterSheet.Describe(obj, x => x.Strength)!;
            var manual = strength.Mods.Single(x => x.IsManual);
            Assert.That(manual.Origin.Kind, Is.EqualTo(RpgOriginKind.Manual));
            Assert.That(manual.Origin.Text, Is.EqualTo("Changed by hand"));
            Assert.That(manual.Counts, Is.True);

            var intelligence = characterSheet.Describe(obj, x => x.Intelligence)!;
            Assert.That(intelligence.Mods.Count(x => x.Origin.Kind == RpgOriginKind.Manual), Is.EqualTo(1));
            Assert.That(intelligence.Mods.Count(x => x.Counts), Is.EqualTo(2));

            Assert.That(characterSheet.GetManualChanges().Select(x => x.Id), Is.EquivalentTo(new[] { over.Id, adjust.Id }));

            //The mark survives a save
            var json = State(characterSheet);
            var characterSheet2 = new RpgCharacterSheet(RpgJson.DeserializeGraphState<RpgCharacterSheetState>(json), characterSheet.GetSystem());
            Assert.That(characterSheet2.GetManualChanges().Length, Is.EqualTo(2));

            characterSheet.RemoveManualChange(over);

            Assert.That(obj.Strength, Is.EqualTo(10));
            Assert.That(characterSheet.GetManualChanges().Select(x => x.Id), Is.EqualTo(new[] { adjust.Id }));
        }

        #endregion Changes made by hand

        #region Rolls

        [Test]
        public void Roll_Pending_IsShown()
        {
            var creature = new TestCreature("Rat");
            var characterSheet = new RpgCharacterSheet(creature);
            characterSheet.RequestRoll(creature, "check", "2d6");

            var description = characterSheet.Describe(creature, "check")!;

            Assert.That(description.IsRollPending, Is.True);
            Assert.That(description.Roll, Is.Null);
            Assert.That(description.Value.ToString(), Is.EqualTo("2d6"));
            Assert.That(description.Mods.Single().Origin.Kind, Is.EqualTo(RpgOriginKind.StartingValue));
            Assert.That(description.ToText(), Does.Contain("roll pending"));
        }

        [Test]
        public void Roll_Stored_ShowsDiceBonusAndWhoRolled()
        {
            var creature = new TestCreature("Rat");
            var characterSheet = new RpgCharacterSheet(creature);
            characterSheet.RequestRoll(creature, "check", "2d6");
            characterSheet.AdjustByHand(creature, "check", 2);
            characterSheet.SetRoll(creature, "check", 7);

            var description = characterSheet.Describe(creature, "check")!;

            Assert.That(description.Value, Is.EqualTo(new Dice(9)));
            Assert.That(description.Expression!.Value.ToString(), Is.EqualTo("2d6 + 2"));
            Assert.That(description.IsRollPending, Is.False);
            Assert.That(description.IsRollOutOfDate, Is.False);
            Assert.That(description.Roll!.Result, Is.EqualTo(7));
            Assert.That(description.Roll.DicePart, Is.EqualTo("2d6"));
            Assert.That(description.Roll.SuppliedBy, Is.EqualTo(RpgRollSource.Player));
            Assert.That(description.ToText(), Does.StartWith("Rat.check = 9 (2d6 + 2: rolled 7 on 2d6 by the player)"));
        }

        [Test]
        public void Roll_OutOfDate_IsShown()
        {
            var creature = new TestCreature("Rat");
            var characterSheet = new RpgCharacterSheet(creature);
            characterSheet.RequestRoll(creature, "check", "2d6");
            characterSheet.SetRoll(creature, "check", 7);
            characterSheet.AdjustByHand(creature, "check", new Dice("1d6"));

            var description = characterSheet.Describe(creature, "check")!;

            Assert.That(description.IsRollPending, Is.True);
            Assert.That(description.IsRollOutOfDate, Is.True);
            Assert.That(description.Roll!.Result, Is.EqualTo(7));
            Assert.That(description.ToText(), Does.Contain("the earlier roll of 7 was on 2d6"));
        }

        #endregion Rolls

        #region States and mod sets

        [Test]
        public void State_ConditionNotMet_ThenMet()
        {
            var obj = new TestObject("Thing");
            var characterSheet = new RpgCharacterSheet(obj);

            var off = characterSheet.DescribeState(obj, nameof(TestState))!;

            Assert.That(off.IsState, Is.True);
            Assert.That(off.Name, Is.EqualTo(nameof(TestState)));
            Assert.That(off.OwnerName, Is.EqualTo("Thing"));
            Assert.That(off.IsOn, Is.False);
            Assert.That(off.Reason, Is.EqualTo(RpgStateReason.ConditionNotMet));

            //What it would change is known even though it is off
            Assert.That(off.Changes.Count, Is.EqualTo(1));
            Assert.That(off.Changes[0].Prop, Is.EqualTo("Initiative"));
            Assert.That(off.Changes[0].Change, Is.EqualTo(new Dice(1)));
            Assert.That(off.Changes[0].Mods.Single().Counts, Is.False);

            characterSheet.AdjustByHand(obj, x => x.Intelligence, 2);

            var on = characterSheet.DescribeState(obj, nameof(TestState))!;

            Assert.That(on.IsOn, Is.True);
            Assert.That(on.Reason, Is.EqualTo(RpgStateReason.ConditionMet));
            Assert.That(on.ReasonText, Is.EqualTo("Its condition is met"));
            Assert.That(on.Changes[0].Mods.Single().Counts, Is.True);
            Assert.That(on.ToText(), Is.EqualTo("State TestState on Thing: on. Its condition is met\n  Thing.Initiative +1".ReplaceLineEndings()));
        }

        [Test]
        public void State_SwitchedByHand()
        {
            var obj = new TestObject("Thing");
            var characterSheet = new RpgCharacterSheet(obj);
            var state = characterSheet.GetObjectState(obj.Id, nameof(TestState))!;

            state.UserEnabled();
            characterSheet.Time.Refresh();

            var on = characterSheet.DescribeState(obj, nameof(TestState))!;
            Assert.That(on.IsOn, Is.True);
            Assert.That(on.Reason, Is.EqualTo(RpgStateReason.SwitchedOnByHand));

            state.UserDisabled();
            characterSheet.Time.Refresh();

            var off = characterSheet.DescribeState(obj, nameof(TestState))!;
            Assert.That(off.IsOn, Is.False);
            Assert.That(off.Reason, Is.EqualTo(RpgStateReason.SwitchedOffByHand));

            var mod = characterSheet.Describe(obj, x => x.Initiative)!.Mods.Single();
            Assert.That(mod.Status, Is.EqualTo(RpgModStatus.SwitchedOff));
            Assert.That(mod.WhyNot, Is.EqualTo("Switched off by hand"));
        }

        [Test]
        public void State_SwitchedOnForATime()
        {
            var obj = new TestObject("Thing");
            var characterSheet = new RpgCharacterSheet(obj);
            characterSheet.BeginTurnTracking();

            characterSheet.ActivateState(obj.Id, nameof(TestState), 2);
            characterSheet.Time.Refresh();

            var description = characterSheet.DescribeState(obj, nameof(TestState))!;

            Assert.That(description.IsOn, Is.True);
            Assert.That(description.Reason, Is.EqualTo(RpgStateReason.Activated));
            Assert.That(description.Activations.Count(x => x.Counts), Is.EqualTo(1));
            Assert.That(description.Activations.Single(x => x.Counts).Lifespan, Is.EqualTo("from turn 1 until turn 3"));
            Assert.That(description.ReasonText, Does.Contain("from turn 1 until turn 3"));

            characterSheet.AdvanceToTurn(3);

            description = characterSheet.DescribeState(obj, nameof(TestState))!;
            Assert.That(description.IsOn, Is.False);
            Assert.That(description.Reason, Is.EqualTo(RpgStateReason.ConditionNotMet));
        }

        [Test]
        public void State_NeedsTurnTracking_IsShown()
        {
            var creature = new TestCreature("Rat");
            var characterSheet = new RpgCharacterSheet(creature);

            var description = characterSheet.DescribeState(creature, nameof(TestBleeding))!;

            Assert.That(description.NeedsTurnTracking, Is.True);
            Assert.That(description.IsOn, Is.False);
        }

        [Test]
        public void ModSet_NotApplied_DescribedWithoutChangingTheSheet()
        {
            var creature = new TestCreature("Rat");
            var characterSheet = new RpgCharacterSheet(creature);

            var modSet = new RpgModSet("Blessing", creature.Id, false);
            modSet.Add(creature, x => x.Bonus, 3);
            modSet.Add(creature, x => x.Health, x => x.Ammo);
            modSet.Unapply();
            characterSheet.Add(modSet);
            characterSheet.Time.Refresh();

            Assert.That(creature.Bonus, Is.EqualTo(0));
            Assert.That(creature.Health, Is.EqualTo(5));

            var before = State(characterSheet);
            var description = characterSheet.DescribeModSet(modSet);

            Assert.That(description.IsState, Is.False);
            Assert.That(description.Name, Is.EqualTo("Blessing"));
            Assert.That(description.IsApplied, Is.False);
            Assert.That(description.Changes.Count, Is.EqualTo(2));
            Assert.That(description.Changes.Single(x => x.Prop == "Bonus").Change, Is.EqualTo(new Dice(3)));
            Assert.That(description.Changes.Single(x => x.Prop == "Health").Change, Is.EqualTo(new Dice(10)));
            Assert.That(description.ToText(), Does.StartWith("Mod set Blessing on Rat: not applied"));

            //Nothing was applied to find out
            Assert.That(State(characterSheet), Is.EqualTo(before));
            Assert.That(creature.Bonus, Is.EqualTo(0));

            //The mods name the set they came from
            var bonus = characterSheet.Describe(creature, x => x.Bonus)!.Mods.Single(x => x.Origin.Kind == RpgOriginKind.ModSet);
            Assert.That(bonus.Origin.Name, Is.EqualTo("Blessing"));
            Assert.That(bonus.Counts, Is.False);
            Assert.That(bonus.WhyNot, Is.EqualTo("Blessing is not applied"));

            modSet.Apply();
            characterSheet.Time.Refresh();

            Assert.That(creature.Bonus, Is.EqualTo(3));
            Assert.That(characterSheet.DescribeModSet(modSet).IsApplied, Is.True);
        }

        #endregion States and mod sets

        #region Actions

        [Test]
        public void Action_InProgress_ShowsInputsAndPendingRolls()
        {
            var creature = new TestCreature("Rat");
            var characterSheet = new RpgCharacterSheet(creature);
            var swing = Start(characterSheet, creature, nameof(TestSwing));

            Assert.That(swing.Perform(characterSheet, ("aim", 1)), Is.True);
            Assert.That(swing.Outcome(characterSheet, ("target", 8)), Is.False);

            var description = characterSheet.DescribeAction(swing);

            Assert.That(description.ActionName, Is.EqualTo(nameof(TestSwing)));
            Assert.That(description.OwnerName, Is.EqualTo("Rat"));
            Assert.That(description.IsComplete, Is.False);
            Assert.That(description.Steps.Select(x => x.Name), Is.EqualTo(new[] { "Cost", "Perform", "Outcome" }));
            Assert.That(description.Steps.Select(x => x.IsDone), Is.EqualTo(new[] { true, true, false }));

            var outcome = description.Steps.Single(x => x.Name == "Outcome");
            Assert.That(outcome.Inputs.Select(x => x.Name), Is.EqualTo(new[] { "owner", "hitRoll", "target" }));
            Assert.That(outcome.Inputs.Single(x => x.Name == "owner").ObjectName, Is.EqualTo("Rat"));
            Assert.That(outcome.Inputs.Single(x => x.Name == "target").Value, Is.EqualTo("8"));

            var hitRoll = outcome.Inputs.Single(x => x.Name == "hitRoll");
            Assert.That(hitRoll.Value, Is.Null);
            Assert.That(hitRoll.IsRollPending, Is.True);

            //The input is described like any other value: the dice and the bonus from aiming
            Assert.That(hitRoll.Property!.Value.ToString(), Is.EqualTo("2d6 + 1"));
            Assert.That(hitRoll.Property.Mods.Count, Is.EqualTo(2));
            Assert.That(hitRoll.Property.Mods.Single(x => x.Kind == nameof(Standard)).Origin.Kind, Is.EqualTo(RpgOriginKind.ActionInput));
            Assert.That(hitRoll.Property.Mods.Single(x => x.Kind == nameof(Standard)).Origin.Name, Is.EqualTo(nameof(TestSwing)));

            Assert.That(description.PendingRolls.Count, Is.EqualTo(1));
            Assert.That(description.PendingRolls[0].Prop, Is.EqualTo("hitRoll"));
            Assert.That(description.Rolls, Is.Empty);
            Assert.That(description.Effects, Is.Empty);
        }

        [Test]
        public void Action_Completed_ShowsRollsAndEffects()
        {
            var creature = new TestCreature("Rat");
            var characterSheet = new RpgCharacterSheet(creature) { DiceRoller = new TestDiceRoller(3, 4) };
            characterSheet.BeginTurnTracking();

            var swing = Start(characterSheet, creature, nameof(TestSwing));
            swing.AutoComplete(characterSheet, ("aim", 1), ("target", 8));

            var description = characterSheet.DescribeAction(swing);

            Assert.That(description.IsComplete, Is.True);
            Assert.That(description.PendingRolls, Is.Empty);
            Assert.That(description.Rolls.Count, Is.EqualTo(1));
            Assert.That(description.Rolls[0].Prop, Is.EqualTo("hitRoll"));
            Assert.That(description.Rolls[0].Expression.ToString(), Is.EqualTo("2d6 + 1"));
            Assert.That(description.Rolls[0].Roll.Result, Is.EqualTo(7));
            Assert.That(description.Rolls[0].Roll.SuppliedBy, Is.EqualTo(RpgRollSource.App));

            Assert.That(description.Effects.Count, Is.EqualTo(1));
            Assert.That(description.Effects[0].ObjectName, Is.EqualTo("Rat"));
            Assert.That(description.Effects[0].Prop, Is.EqualTo("Bonus"));
            Assert.That(description.Effects[0].Change, Is.EqualTo(new Dice(8)));

            //The effect on the creature names the action it came from
            var bonus = characterSheet.Describe(creature, x => x.Bonus)!.Mods.Single(x => x.Counts);
            Assert.That(bonus.Origin.Kind, Is.EqualTo(RpgOriginKind.ActionEffect));
            Assert.That(bonus.Origin.Name, Is.EqualTo(nameof(TestSwing)));
            Assert.That(bonus.Origin.Text, Is.EqualTo("Effect of TestSwing"));
        }

        [Test]
        public void Action_Costs_NameTheAction()
        {
            var creature = new TestCreature("Rat");
            var characterSheet = new RpgCharacterSheet(creature);
            characterSheet.BeginTurnTracking();

            var strike = Start(characterSheet, creature, nameof(TestStrike));
            strike.AutoComplete(characterSheet, ("effectTurns", 2), ("trivial", 0));

            var description = characterSheet.DescribeAction(strike);

            Assert.That(description.Costs.Count, Is.EqualTo(2));
            Assert.That(description.Costs.Single(x => x.Prop == "Actions").Change, Is.EqualTo(new Dice(-1)));
            Assert.That(description.Costs.Single(x => x.Prop == "Actions").Lifespan, Is.EqualTo("from turn 1 until turn 2"));
            Assert.That(description.Costs.Single(x => x.Prop == "Ammo").Lifespan, Is.EqualTo("permanent"));
            Assert.That(description.SkippedCosts, Is.Empty);
            Assert.That(description.DroppedEffects, Is.Empty);
            Assert.That(description.Effects.Single().Lifespan, Is.EqualTo("from turn 1 until turn 3"));

            var actions = characterSheet.Describe(creature, x => x.Actions)!;
            var cost = actions.Mods.Single(x => x.Origin.Kind == RpgOriginKind.ActionCost);

            Assert.That(actions.Value, Is.EqualTo(Dice.Zero));
            Assert.That(cost.Origin.Name, Is.EqualTo(nameof(TestStrike)));
            Assert.That(cost.Origin.Step, Is.EqualTo("Cost"));
            Assert.That(cost.Origin.Text, Is.EqualTo("Cost of TestStrike"));
            Assert.That(cost.Contribution, Is.EqualTo(new Dice(-1)));
        }

        [Test]
        public void Action_OutsideTurnTracking_SkippedCostsAndDroppedEffectsHaveReasons()
        {
            var creature = new TestCreature("Rat");
            var characterSheet = new RpgCharacterSheet(creature);

            var strike = Start(characterSheet, creature, nameof(TestStrike));
            strike.AutoComplete(characterSheet, ("effectTurns", 2), ("trivial", 1));

            Assert.That(characterSheet.Time.IsTurnTracking, Is.False);
            Assert.That(strike.DroppedEffects.Count, Is.EqualTo(1));

            var description = characterSheet.DescribeAction(strike);

            //The round of ammunition is gone for good and was charged
            Assert.That(description.Costs.Select(x => x.Prop), Is.EqualTo(new[] { "Ammo" }));

            //This turn's action was not charged, because nobody is counting turns
            Assert.That(description.SkippedCosts.Count, Is.EqualTo(1));
            Assert.That(description.SkippedCosts[0].Prop, Is.EqualTo("Actions"));
            Assert.That(description.SkippedCosts[0].Change, Is.EqualTo(new Dice(-1)));
            Assert.That(description.SkippedCosts[0].Reason, Is.EqualTo(RpgDescriber.SkippedCostReason));

            //The minor effect was dropped rather than starting turn tracking
            Assert.That(description.Effects, Is.Empty);
            Assert.That(description.DroppedEffects.Count, Is.EqualTo(1));
            Assert.That(description.DroppedEffects[0].Prop, Is.EqualTo("Bonus"));
            Assert.That(description.DroppedEffects[0].Change, Is.EqualTo(new Dice(2)));
            Assert.That(description.DroppedEffects[0].Reason, Is.EqualTo(RpgDescriber.DroppedEffectReason));

            var text = description.ToText();
            Assert.That(text, Does.Contain("Costs skipped:"));
            Assert.That(text, Does.Contain("Effects dropped:"));

            //The record of what was dropped is saved with the sheet
            var characterSheet2 = new RpgCharacterSheet(RpgJson.DeserializeGraphState<RpgCharacterSheetState>(State(characterSheet)), characterSheet.GetSystem());
            var strike2 = (characterSheet2.GetObject(strike.Id) as RpgActivityAction)!;
            Assert.That(characterSheet2.DescribeAction(strike2).DroppedEffects.Count, Is.EqualTo(1));

            //Undoing the action forgets it
            strike.Reset(characterSheet);
            Assert.That(strike.DroppedEffects, Is.Empty);
        }

        [Test]
        public void Action_InsideTurnTracking_MinorEffectRuns()
        {
            var creature = new TestCreature("Rat");
            var characterSheet = new RpgCharacterSheet(creature);
            characterSheet.BeginTurnTracking();

            var strike = Start(characterSheet, creature, nameof(TestStrike));
            strike.AutoComplete(characterSheet, ("effectTurns", 2), ("trivial", 1));

            var description = characterSheet.DescribeAction(strike);

            Assert.That(description.DroppedEffects, Is.Empty);
            Assert.That(description.Effects.Count, Is.EqualTo(1));
            Assert.That(creature.Bonus, Is.EqualTo(2));
        }

        [Test]
        public void Action_FollowOnActions_AreListed()
        {
            var obj = new TestObject("Thing");
            var characterSheet = new RpgCharacterSheet(obj);
            var activityAction = characterSheet.CreateActivity(obj.Id, obj.Id, nameof(TestAction)).CurrentActivityAction!;

            activityAction.AutoComplete(characterSheet, ("value", 1));

            var description = characterSheet.DescribeAction(activityAction);

            Assert.That(description.IsComplete, Is.True);
            Assert.That(description.FollowOnActions.Count, Is.EqualTo(1));
            Assert.That(description.FollowOnActions[0].ActionName, Is.EqualTo(nameof(TestAction)));
            Assert.That(description.ToText(), Does.Contain("Follow-on actions:"));
        }

        #endregion Actions

        #region Objects

        [Test]
        public void Object_Summary()
        {
            var obj = new TestObject("Thing");
            var characterSheet = new RpgCharacterSheet(obj);
            characterSheet.OverrideByHand(obj, x => x.Strength, 16);
            characterSheet.RequestRoll(obj, "check", "2d6");

            var description = characterSheet.DescribeObject(obj);

            Assert.That(description.Name, Is.EqualTo("Thing"));
            Assert.That(description.Archetype, Is.EqualTo(nameof(TestObject)));

            var strength = description.Properties.Single(x => x.Prop == "Strength");
            Assert.That(strength.DisplayName, Is.EqualTo("Str"));
            Assert.That(strength.Value, Is.EqualTo(new Dice(16)));
            Assert.That(strength.OriginalValue, Is.EqualTo(new Dice(10)));
            Assert.That(strength.IsChanged, Is.True);

            var bonus = description.Properties.Single(x => x.Prop == "StrengthBonus");
            Assert.That(bonus.Value, Is.EqualTo(new Dice(3)));
            Assert.That(bonus.IsChanged, Is.False);

            Assert.That(description.Properties.Single(x => x.Prop == "check").IsRollPending, Is.True);
            Assert.That(description.Properties.Any(x => x.Prop.StartsWith("State/")), Is.False);

            Assert.That(description.StatesOn, Is.Empty);
            Assert.That(description.StatesOff, Is.EqualTo(new[] { nameof(TestState) }));
        }

        #endregion Objects

        #region Describing only reads

        [Test]
        public void Describe_NeverChangesTheSheet_AndNeverRolls()
        {
            var creature = new TestCreature("Rat");
            var roller = new TestDiceRoller();
            var characterSheet = new RpgCharacterSheet(creature) { DiceRoller = roller };
            characterSheet.BeginTurnTracking();

            var strike = Start(characterSheet, creature, nameof(TestStrike));
            strike.AutoComplete(characterSheet, ("effectTurns", 2), ("trivial", 0));

            var swing = Start(characterSheet, creature, nameof(TestSwing));
            swing.Perform(characterSheet, ("aim", 1));
            characterSheet.RequestRoll(creature, "check", "2d6");
            characterSheet.Time.Refresh();

            var before = State(characterSheet);
            var changed = characterSheet.GetChangedProperties().Length;

            foreach (var prop in new[] { "Health", "Actions", "Ammo", "Bonus", "check" })
                Assert.That(characterSheet.Describe(creature, prop), Is.Not.Null);

            characterSheet.Describe(swing, "hitRoll");
            characterSheet.DescribeAction(strike);
            characterSheet.DescribeAction(swing);
            characterSheet.DescribeObject(creature);
            characterSheet.DescribeState(creature, nameof(TestBleeding));

            foreach (var modSet in new[] { strike.CostSet, strike.Result, swing.Result })
                characterSheet.DescribeModSet(modSet);

            Assert.That(State(characterSheet), Is.EqualTo(before));
            Assert.That(characterSheet.GetChangedProperties().Length, Is.EqualTo(changed));
            Assert.That(roller.Rolled, Is.EqualTo(0));
            Assert.That(characterSheet.GetPendingRolls().Length, Is.EqualTo(2));
        }

        #endregion Describing only reads

        #region Text and data

        [Test]
        public void Text_Property()
        {
            var obj = new TestObject("Thing");
            var characterSheet = new RpgCharacterSheet(obj);
            characterSheet.OverrideByHand(obj, x => x.Strength, 16);

            var text = characterSheet.Describe(obj, x => x.StrengthBonus)!.ToText();

            var expected = string.Join(Environment.NewLine,
                "Thing.StrengthBonus = 3",
                "  +3 Base: From Thing.Str",
                "      calculation: Half of the score above ten, rounded down (16 gives 3)",
                "      Thing.Str = 16",
                "        +10 Initial: Starting value [not counted: Set aside by an override]",
                "        +16 Override: Changed by hand");

            Assert.That(text, Is.EqualTo(expected));
        }

        [Test]
        public void Text_Action()
        {
            var creature = new TestCreature("Rat");
            var characterSheet = new RpgCharacterSheet(creature);
            var swing = Start(characterSheet, creature, nameof(TestSwing));

            swing.Perform(characterSheet, ("aim", 1));
            swing.Outcome(characterSheet, ("target", 8));

            var expected = string.Join(Environment.NewLine,
                "TestSwing (Rat): in progress",
                "  Cost (done)",
                "  Perform (done)",
                "    aim = 1",
                "  Outcome",
                "    owner = Rat",
                "    hitRoll = (roll pending)",
                "    target = 8",
                "  Rolls pending:",
                "    hitRoll: 2d6 + 1");

            Assert.That(characterSheet.DescribeAction(swing).ToText(), Is.EqualTo(expected));
        }

        [Test]
        public void Data_SurvivesBeingTurnedIntoTextAndBack()
        {
            var creature = new TestCreature("Rat");
            var characterSheet = new RpgCharacterSheet(creature) { DiceRoller = new TestDiceRoller(3, 4) };
            characterSheet.BeginTurnTracking();

            var swing = Start(characterSheet, creature, nameof(TestSwing));
            swing.AutoComplete(characterSheet, ("aim", 1), ("target", 8));
            characterSheet.OverrideByHand(creature, x => x.Health, 2);

            var property = characterSheet.Describe(swing, "hitRoll")!;
            var property2 = JsonConvert.DeserializeObject<RpgPropertyDescription>(JsonConvert.SerializeObject(property))!;
            Assert.That(property2.ToText(), Is.EqualTo(property.ToText()));
            Assert.That(property2.Roll!.Result, Is.EqualTo(7));

            var action = characterSheet.DescribeAction(swing);
            var action2 = JsonConvert.DeserializeObject<RpgActionDescription>(JsonConvert.SerializeObject(action))!;
            Assert.That(action2.ToText(), Is.EqualTo(action.ToText()));

            var state = characterSheet.DescribeState(creature, nameof(TestBleeding))!;
            var state2 = JsonConvert.DeserializeObject<RpgModSetDescription>(JsonConvert.SerializeObject(state))!;
            Assert.That(state2.ToText(), Is.EqualTo(state.ToText()));
            Assert.That(state2.IsOn, Is.True);

            var obj = characterSheet.DescribeObject(creature);
            var obj2 = JsonConvert.DeserializeObject<RpgObjectDescription>(JsonConvert.SerializeObject(obj))!;
            Assert.That(obj2.ToText(), Is.EqualTo(obj.ToText()));
        }

        #endregion Text and data
    }
}
