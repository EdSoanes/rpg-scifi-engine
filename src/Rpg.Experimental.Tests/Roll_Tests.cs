using Rpg.Experimental.Json;
using Rpg.Experimental.Mods;
using Rpg.Experimental.Reflection;
using Rpg.Experimental.Reflection.Args;
using Rpg.Experimental.Tests.Models;

namespace Rpg.Experimental.Tests
{
    /// <summary>
    /// Dice rolls as described in feature 3 of docs/VISION.md: reading a value never rolls. A roll that is
    /// needed is pending until the app rolls it or the player supplies the result, and it is then stored.
    /// </summary>
    public class Roll_Tests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeUtilities.RegisterAssembly(typeof(TestObject).Assembly);
        }

        private static RpgCharacterSheet Sheet(RpgObject actor, params int[] diceResults)
            => new RpgCharacterSheet(actor) { DiceRoller = new TestDiceRoller(diceResults) };

        private static TestDiceRoller Roller(RpgCharacterSheet characterSheet)
            => (TestDiceRoller)characterSheet.DiceRoller;

        private static RpgCharacterSheet RoundTrip(RpgCharacterSheet characterSheet)
        {
            var json = RpgJson.SerializeGraphState(characterSheet.GetState());
            var state = RpgJson.DeserializeGraphState<RpgCharacterSheetState>(json);

            return new RpgCharacterSheet(state, characterSheet.GetSystem()) { DiceRoller = characterSheet.DiceRoller };
        }

        private static RpgActivityAction Swing(RpgCharacterSheet characterSheet, TestCreature creature)
            => characterSheet.CreateActivity(creature.Id, creature.Id, nameof(TestSwing)).CurrentActivityAction!;

        #region Reading never rolls

        [Test]
        public void Read_DiceValue_NeverRolls()
        {
            var obj = new TestObject();
            var characterSheet = Sheet(obj);

            for (var i = 0; i < 20; i++)
                Assert.That(characterSheet.GetPropertyValue<Dice>(obj, "Damage").ToString(), Is.EqualTo("1d6 + 11"));

            characterSheet.BeginTurnTracking();
            characterSheet.AdvanceToTurn(4);
            characterSheet.EndTurnTracking();
            characterSheet.TriggerTimeEvent("Sunrise");

            Assert.That(obj.Damage.ToString(), Is.EqualTo("1d6 + 11"));
            Assert.That(Roller(characterSheet).Rolled, Is.EqualTo(0));

            //A stat that holds dice is not a roll waiting to be made
            Assert.That(characterSheet.GetPendingRolls(), Is.Empty);
        }

        [Test]
        public void Read_UnrolledValueAsNumber_IsAnError()
        {
            var creature = new TestCreature();
            var characterSheet = Sheet(creature);

            characterSheet.RequestRoll(creature, "check", "2d6");

            Assert.That(characterSheet.GetPropertyValue<Dice>(creature, "check").ToString(), Is.EqualTo("2d6"));
            Assert.That(characterSheet.GetPropertyValue<int?>(creature, "check"), Is.Null);
            Assert.Throws<RpgUnrolledDiceException>(() => characterSheet.GetPropertyValue<int>(creature, "check"));
            Assert.That(Roller(characterSheet).Rolled, Is.EqualTo(0));
        }

        #endregion Reading never rolls

        #region Stored rolls

        [Test]
        public void Roll_ByApp_IsStoredOnce()
        {
            var creature = new TestCreature();
            var characterSheet = Sheet(creature, 3, 4);

            characterSheet.RequestRoll(creature, "check", "2d6");

            var pending = characterSheet.GetPendingRolls();
            Assert.That(pending.Length, Is.EqualTo(1));
            Assert.That(pending[0].ObjectId, Is.EqualTo(creature.Id));
            Assert.That(pending[0].Prop, Is.EqualTo("check"));
            Assert.That(pending[0].Expression.ToString(), Is.EqualTo("2d6"));
            Assert.That(pending[0].DicePart.ToString(), Is.EqualTo("2d6"));

            var roll = characterSheet.Roll(creature, "check");

            Assert.That(roll, Is.Not.Null);
            Assert.That(roll!.Result, Is.EqualTo(7));
            Assert.That(roll.Dice, Is.EqualTo(new[] { 3, 4 }));
            Assert.That(roll.SuppliedBy, Is.EqualTo(RpgRollSource.App));
            Assert.That(roll.IsOutOfRange, Is.False);
            Assert.That(characterSheet.GetPendingRolls(), Is.Empty);

            for (var i = 0; i < 5; i++)
                Assert.That(characterSheet.GetPropertyValue<int>(creature, "check"), Is.EqualTo(7));

            //Time moving on does not roll again
            characterSheet.BeginTurnTracking();
            characterSheet.AdvanceToTurn(3);
            characterSheet.EndTurnTracking();
            characterSheet.TriggerTimeEvent("Sunset");

            Assert.That(characterSheet.GetPropertyValue<int>(creature, "check"), Is.EqualTo(7));
            Assert.That(Roller(characterSheet).Rolled, Is.EqualTo(2));
        }

        [Test]
        public void Roll_ByPlayer_BehavesLikeAnAppRoll()
        {
            var creature = new TestCreature();
            var characterSheet = Sheet(creature);

            characterSheet.RequestRoll(creature, "check", "2d6");
            var roll = characterSheet.SetRoll(creature, "check", 7);

            Assert.That(roll!.Result, Is.EqualTo(7));
            Assert.That(roll.Dice, Is.Null);
            Assert.That(roll.SuppliedBy, Is.EqualTo(RpgRollSource.Player));
            Assert.That(characterSheet.GetPendingRolls(), Is.Empty);
            Assert.That(characterSheet.GetPropertyValue<int>(creature, "check"), Is.EqualTo(7));

            characterSheet.BeginTurnTracking();
            characterSheet.AdvanceToTurn(3);

            Assert.That(characterSheet.GetPropertyValue<int>(creature, "check"), Is.EqualTo(7));
            Assert.That(Roller(characterSheet).Rolled, Is.EqualTo(0));
        }

        [Test]
        public void Roll_CanBeRedoneAndReplaced()
        {
            var creature = new TestCreature();
            var characterSheet = Sheet(creature, 1, 2, 6, 6);

            characterSheet.RequestRoll(creature, "check", "2d6");

            characterSheet.Roll(creature, "check");
            Assert.That(characterSheet.GetPropertyValue<int>(creature, "check"), Is.EqualTo(3));

            characterSheet.Roll(creature, "check");
            Assert.That(characterSheet.GetPropertyValue<int>(creature, "check"), Is.EqualTo(12));

            characterSheet.SetRoll(creature, "check", 8);
            Assert.That(characterSheet.GetPropertyValue<int>(creature, "check"), Is.EqualTo(8));
            Assert.That(characterSheet.GetRoll(creature.Id, "check")!.SuppliedBy, Is.EqualTo(RpgRollSource.Player));
        }

        [Test]
        public void Roll_Cleared_IsPendingAgain()
        {
            var creature = new TestCreature();
            var characterSheet = Sheet(creature);

            characterSheet.RequestRoll(creature, "check", "2d6");
            characterSheet.SetRoll(creature, "check", 7);

            Assert.That(characterSheet.ClearRoll(creature, "check"), Is.True);
            Assert.That(characterSheet.GetRoll(creature.Id, "check"), Is.Null);
            Assert.That(characterSheet.GetPendingRolls().Length, Is.EqualTo(1));
            Assert.That(characterSheet.GetPropertyValue<Dice>(creature, "check").ToString(), Is.EqualTo("2d6"));
        }

        [Test]
        public void Roll_OutOfRange_IsAcceptedAndFlagged()
        {
            var creature = new TestCreature();
            var characterSheet = Sheet(creature);

            characterSheet.RequestRoll(creature, "check", "2d6");
            var roll = characterSheet.SetRoll(creature, "check", 14);

            Assert.That(roll!.IsOutOfRange, Is.True);
            Assert.That(characterSheet.GetPropertyValue<int>(creature, "check"), Is.EqualTo(14));
        }

        [Test]
        public void Roll_ValueWithoutDice_NothingToRoll()
        {
            var creature = new TestCreature();
            var characterSheet = Sheet(creature);

            Assert.That(characterSheet.Roll(creature, nameof(TestCreature.Health)), Is.Null);
            Assert.That(characterSheet.SetRoll(creature, nameof(TestCreature.Health), 3), Is.Null);
            Assert.That(creature.Health, Is.EqualTo(5));
            Assert.That(Roller(characterSheet).Rolled, Is.EqualTo(0));
        }

        [Test]
        public void Roll_BonusAddedAfterwards_KeepsTheDice()
        {
            var creature = new TestCreature();
            var characterSheet = Sheet(creature);

            characterSheet.RequestRoll(creature, "check", "2d6");
            characterSheet.SetRoll(creature, "check", 7);

            characterSheet.Add(new Standard(), creature, "check", 2);
            characterSheet.Time.Refresh();

            Assert.That(characterSheet.GetPropertyValue<int>(creature, "check"), Is.EqualTo(9));
            Assert.That(characterSheet.GetRoll(creature.Id, "check")!.Result, Is.EqualTo(7));
            Assert.That(characterSheet.GetPendingRolls(), Is.Empty);

            var property = characterSheet.GetPropertyData(creature.Id, "check")!.GetProperty(characterSheet);
            Assert.That(property.Expression.ToString(), Is.EqualTo("2d6 + 2"));
            Assert.That(property.Value.ToString(), Is.EqualTo("9"));
            Assert.That(property.Roll!.Result, Is.EqualTo(7));
            Assert.That(property.IsRollPending, Is.False);
        }

        [Test]
        public void Roll_DicePartChanged_IsPendingAgain()
        {
            var creature = new TestCreature();
            var characterSheet = Sheet(creature);

            characterSheet.RequestRoll(creature, "check", "2d6");
            characterSheet.SetRoll(creature, "check", 7);

            characterSheet.Add(new Standard(), creature, "check", new Dice("1d6"));
            characterSheet.Time.Refresh();

            Assert.That(characterSheet.GetPropertyValue<Dice>(creature, "check").ToString(), Is.EqualTo("3d6"));
            Assert.That(characterSheet.GetPropertyValue<int?>(creature, "check"), Is.Null);

            var pending = characterSheet.GetPendingRolls();
            Assert.That(pending.Length, Is.EqualTo(1));
            Assert.That(pending[0].DicePart.ToString(), Is.EqualTo("3d6"));

            //The old result is kept so it can be shown
            Assert.That(pending[0].OutOfDateRoll, Is.Not.Null);
            Assert.That(pending[0].OutOfDateRoll!.DicePart, Is.EqualTo("2d6"));
            Assert.That(pending[0].OutOfDateRoll!.Result, Is.EqualTo(7));

            characterSheet.SetRoll(creature, "check", 11);
            Assert.That(characterSheet.GetPropertyValue<int>(creature, "check"), Is.EqualTo(11));
            Assert.That(characterSheet.GetPendingRolls(), Is.Empty);
        }

        [Test]
        public void Roll_SurvivesSaveAndRestore()
        {
            var creature = new TestCreature();
            var characterSheet = Sheet(creature, 2, 5);

            characterSheet.RequestRoll(creature, "check", "2d6");
            characterSheet.RequestRoll(creature, "other", "1d8");
            characterSheet.Roll(creature, "check");

            var characterSheet2 = RoundTrip(characterSheet);

            var roll = characterSheet2.GetRoll(creature.Id, "check");
            Assert.That(roll, Is.Not.Null);
            Assert.That(roll!.Result, Is.EqualTo(7));
            Assert.That(roll.Dice, Is.EqualTo(new[] { 2, 5 }));
            Assert.That(roll.DicePart, Is.EqualTo("2d6"));
            Assert.That(roll.SuppliedBy, Is.EqualTo(RpgRollSource.App));
            Assert.That(characterSheet2.GetPropertyValue<int>(creature.Id, "check"), Is.EqualTo(7));

            //The roll that was not made is still pending
            var pending = characterSheet2.GetPendingRolls();
            Assert.That(pending.Length, Is.EqualTo(1));
            Assert.That(pending[0].Prop, Is.EqualTo("other"));
            Assert.That(Roller(characterSheet2).Rolled, Is.EqualTo(2));
        }

        [Test]
        public void Roll_GoingBackATurn_RestoresTheRolls()
        {
            var creature = new TestCreature();
            var characterSheet = Sheet(creature, 3, 4);

            characterSheet.RequestRoll(creature, "check", "2d6");
            characterSheet.BeginTurnTracking();
            characterSheet.NextTurn();

            //Rolled during turn 2
            characterSheet.Roll(creature, "check");
            characterSheet.NextTurn();

            Assert.That(characterSheet.RewindToTurn(3), Is.True);
            Assert.That(characterSheet.GetPropertyValue<int>(creature.Id, "check"), Is.EqualTo(7));
            Assert.That(characterSheet.GetPendingRolls(), Is.Empty);

            //At the start of turn 2 the roll had not been made
            Assert.That(characterSheet.RewindToTurn(2), Is.True);
            Assert.That(characterSheet.GetRoll(creature.Id, "check"), Is.Null);
            Assert.That(characterSheet.GetPendingRolls().Length, Is.EqualTo(1));
        }

        #endregion Stored rolls

        #region Whole number properties

        [Test]
        public void IntProperty_GivenDice_WaitsForTheRoll()
        {
            var obj = new TestObject();
            var characterSheet = Sheet(obj);

            Assert.That(obj.Strength, Is.EqualTo(10));
            Assert.That(obj.StrengthBonus, Is.EqualTo(0));

            //The sheet never refuses: a stat can be given dice. It has no number until they are rolled.
            characterSheet.Add(new Override(), obj, x => x.Strength, new Dice("3d6"));
            characterSheet.Time.Refresh();

            Assert.That(obj.Strength, Is.EqualTo(10));
            Assert.That(obj.StrengthBonus, Is.EqualTo(0));

            var pending = characterSheet.GetPendingRolls();
            Assert.That(pending.Length, Is.EqualTo(1));
            Assert.That(pending[0].Prop, Is.EqualTo(nameof(TestObject.Strength)));
            Assert.That(pending[0].Expression.ToString(), Is.EqualTo("3d6"));

            characterSheet.SetRoll(obj, nameof(TestObject.Strength), 16);

            Assert.That(obj.Strength, Is.EqualTo(16));
            Assert.That(obj.StrengthBonus, Is.EqualTo(3));
            Assert.That(characterSheet.GetPendingRolls(), Is.Empty);
        }

        #endregion Whole number properties

        #region Actions

        [Test]
        public void Action_PendingRoll_StepDoesNotRun()
        {
            var creature = new TestCreature();
            var characterSheet = Sheet(creature);
            var swing = Swing(characterSheet, creature);

            Assert.That(characterSheet.RollMode, Is.EqualTo(RpgRollMode.Ask));
            Assert.That(swing.Perform(characterSheet, ("aim", 1)), Is.True);
            Assert.That(swing.Outcome(characterSheet, ("target", 8)), Is.False);
            Assert.That(swing.OutcomeMethod.IsDone, Is.False);

            var pending = swing.GetPendingRolls(characterSheet);
            Assert.That(pending.Length, Is.EqualTo(1));
            Assert.That(pending[0].Prop, Is.EqualTo("hitRoll"));
            Assert.That(pending[0].Expression.ToString(), Is.EqualTo("2d6 + 1"));
            Assert.That(pending[0].DicePart.ToString(), Is.EqualTo("2d6"));
            Assert.That(pending[0].ActionName, Is.EqualTo(nameof(TestSwing)));
            Assert.That(pending[0].Steps, Is.EqualTo(new[] { ActionMethodNames.Outcome }));

            Assert.That(swing.GetPendingRolls(characterSheet, ActionMethodNames.Perform), Is.Empty);
            Assert.That(characterSheet.GetPendingRolls().Length, Is.EqualTo(1));
            Assert.That(Roller(characterSheet).Rolled, Is.EqualTo(0));

            //The player rolls real dice
            characterSheet.SetRoll(swing, "hitRoll", 8);

            Assert.That(swing.Outcome(characterSheet), Is.True);
            swing.Complete(characterSheet);

            Assert.That(creature.Bonus, Is.EqualTo(9));
            Assert.That(characterSheet.GetPendingRolls(), Is.Empty);
        }

        [Test]
        public void Action_RollNotRerolledByTurns()
        {
            var creature = new TestCreature();
            var characterSheet = Sheet(creature, 2, 3);
            characterSheet.BeginTurnTracking();

            var swing = Swing(characterSheet, creature);
            Assert.That(swing.Perform(characterSheet, ("aim", 0)), Is.True);

            characterSheet.Roll(swing, "hitRoll");
            characterSheet.Time.Refresh();
            characterSheet.Time.Refresh();

            Assert.That(swing.OutcomeMethod.Args.Find("hitRoll")?.Value, Is.EqualTo(5));
            Assert.That(swing.Outcome(characterSheet, ("target", 5)), Is.True);
            swing.Complete(characterSheet);

            characterSheet.AdvanceToTurn(4);

            Assert.That(creature.Bonus, Is.EqualTo(5));
            Assert.That(Roller(characterSheet).Rolled, Is.EqualTo(2));
        }

        [Test]
        public void Action_NumberSuppliedWithStep_IsTheResultOfTheDice()
        {
            var creature = new TestCreature();
            var characterSheet = Sheet(creature);
            var swing = Swing(characterSheet, creature);

            Assert.That(swing.Perform(characterSheet, ("aim", 2)), Is.True);
            Assert.That(swing.Outcome(characterSheet, ("hitRoll", 6), ("target", 8)), Is.True);

            var roll = characterSheet.GetRoll(swing.Id, "hitRoll");
            Assert.That(roll!.Result, Is.EqualTo(6));
            Assert.That(roll.DicePart, Is.EqualTo("2d6"));
            Assert.That(roll.SuppliedBy, Is.EqualTo(RpgRollSource.Player));
            Assert.That(characterSheet.GetPropertyValue<Dice>(swing.Id, "hitRoll").ToString(), Is.EqualTo("8"));

            swing.Complete(characterSheet);
            Assert.That(creature.Bonus, Is.EqualTo(8));
        }

        [Test]
        public void Action_AppRolls_WhenTheSheetSaysSo()
        {
            var creature = new TestCreature();
            var characterSheet = Sheet(creature, 5, 5);
            characterSheet.RollMode = RpgRollMode.App;

            var swing = Swing(characterSheet, creature);

            Assert.That(swing.Perform(characterSheet, ("aim", 1)), Is.True);
            Assert.That(Roller(characterSheet).Rolled, Is.EqualTo(0));

            Assert.That(swing.Outcome(characterSheet, ("target", 8)), Is.True);

            var roll = characterSheet.GetRoll(swing.Id, "hitRoll");
            Assert.That(roll!.Result, Is.EqualTo(10));
            Assert.That(roll.Dice, Is.EqualTo(new[] { 5, 5 }));
            Assert.That(roll.SuppliedBy, Is.EqualTo(RpgRollSource.App));

            swing.Complete(characterSheet);
            Assert.That(creature.Bonus, Is.EqualTo(11));
        }

        [Test]
        public void Action_AppRolls_DoesNotReplaceThePlayersRoll()
        {
            var creature = new TestCreature();
            var characterSheet = Sheet(creature, 5, 5);
            characterSheet.RollMode = RpgRollMode.App;

            var swing = Swing(characterSheet, creature);
            characterSheet.SetRoll(swing, "hitRoll", 4);

            Assert.That(swing.Perform(characterSheet, ("aim", 1)), Is.True);
            Assert.That(swing.Outcome(characterSheet, ("target", 3)), Is.True);
            swing.Complete(characterSheet);

            Assert.That(creature.Bonus, Is.EqualTo(5));
            Assert.That(Roller(characterSheet).Rolled, Is.EqualTo(0));
        }

        [Test]
        public void Action_AutoComplete_RollsWhatIsPending()
        {
            var creature = new TestCreature();
            var characterSheet = Sheet(creature, 6, 2);
            var swing = Swing(characterSheet, creature);

            //The sheet is set to ask, but auto completing always lets the app roll
            Assert.That(characterSheet.RollMode, Is.EqualTo(RpgRollMode.Ask));
            Assert.That(swing.CanAutoCompleteWithRolls(characterSheet), Is.False);

            swing.AutoComplete(characterSheet, ("aim", 1), ("target", 8));

            Assert.That(swing.IsComplete, Is.True);
            Assert.That(creature.Bonus, Is.EqualTo(9));

            //The roll can be seen afterwards
            var roll = characterSheet.GetRoll(swing.Id, "hitRoll");
            Assert.That(roll!.Result, Is.EqualTo(8));
            Assert.That(roll.Dice, Is.EqualTo(new[] { 6, 2 }));
            Assert.That(roll.SuppliedBy, Is.EqualTo(RpgRollSource.App));
        }

        [Test]
        public void Action_AutoComplete_MissingInput_DoesNothing()
        {
            var creature = new TestCreature();
            var characterSheet = Sheet(creature, 6, 2);
            var swing = Swing(characterSheet, creature);

            //No target
            swing.AutoComplete(characterSheet, ("aim", 1));

            Assert.That(swing.IsComplete, Is.False);
            Assert.That(swing.PerformMethod.IsDone, Is.False);
            Assert.That(Roller(characterSheet).Rolled, Is.EqualTo(0));
        }

        [Test]
        public void Action_ResetStep_KeepsTheRoll_ResetAction_ClearsIt()
        {
            var creature = new TestCreature();
            var characterSheet = Sheet(creature);
            var swing = Swing(characterSheet, creature);

            Assert.That(swing.Perform(characterSheet, ("aim", 1)), Is.True);
            Assert.That(swing.Outcome(characterSheet, ("hitRoll", 7), ("target", 8)), Is.True);

            //Redo the perform step with a better aim. The dice were rolled whatever the bonus is.
            swing.Reset(characterSheet, ActionMethodNames.Perform);
            Assert.That(swing.Perform(characterSheet, ("aim", 3)), Is.True);

            Assert.That(characterSheet.GetRoll(swing.Id, "hitRoll")!.Result, Is.EqualTo(7));
            Assert.That(characterSheet.GetPropertyValue<Dice>(swing.Id, "hitRoll").ToString(), Is.EqualTo("10"));

            swing.Reset(characterSheet);

            Assert.That(characterSheet.GetRoll(swing.Id, "hitRoll"), Is.Null);
            Assert.That(characterSheet.GetPropertyValue<Dice>(swing.Id, "hitRoll").IsConstant, Is.False);
            Assert.That(swing.GetPendingRolls(characterSheet).Length, Is.EqualTo(1));
        }

        [Test]
        public void Action_PendingRoll_SurvivesSaveAndRestore()
        {
            var creature = new TestCreature();
            var characterSheet = Sheet(creature);
            var swing = Swing(characterSheet, creature);
            Assert.That(swing.Perform(characterSheet, ("aim", 1)), Is.True);

            var characterSheet2 = RoundTrip(characterSheet);
            var swing2 = (characterSheet2.GetObject(swing.Id) as RpgActivityAction)!;

            var pending = swing2.GetPendingRolls(characterSheet2);
            Assert.That(pending.Length, Is.EqualTo(1));
            Assert.That(pending[0].Expression.ToString(), Is.EqualTo("2d6 + 1"));
            Assert.That(swing2.Outcome(characterSheet2, ("target", 8)), Is.False);

            characterSheet2.SetRoll(swing2, "hitRoll", 9);
            Assert.That(swing2.Outcome(characterSheet2), Is.True);

            swing2.Complete(characterSheet2);
            Assert.That((characterSheet2.Actor as TestCreature)!.Bonus, Is.EqualTo(10));
        }

        #endregion Actions

        #region Who rolls

        [Test]
        public void RollMode_IsSavedWithTheSheet_AndNotRewound()
        {
            var creature = new TestCreature();
            var characterSheet = Sheet(creature);

            characterSheet.RollMode = RpgRollMode.App;
            Assert.That(RoundTrip(characterSheet).RollMode, Is.EqualTo(RpgRollMode.App));

            characterSheet.RollMode = RpgRollMode.Ask;
            characterSheet.BeginTurnTracking();
            characterSheet.NextTurn();
            characterSheet.RollMode = RpgRollMode.App;

            //Who rolls is a setting, not something that happened in a turn
            Assert.That(characterSheet.RewindToTurn(1), Is.True);
            Assert.That(characterSheet.RollMode, Is.EqualTo(RpgRollMode.App));
        }

        #endregion Who rolls

        #region Rolls caused by time

        [Test]
        public void TimeCausedRoll_IsPending_AndTimeIsNotHeldUp()
        {
            var creature = new TestCreature { RollEachTurn = 1 };
            var characterSheet = Sheet(creature, 2, 3, 4, 1);

            Assert.That(characterSheet.GetPendingRolls(), Is.Empty);

            characterSheet.BeginTurnTracking();

            var pending = characterSheet.GetPendingRolls();
            Assert.That(pending.Length, Is.EqualTo(1));
            Assert.That(pending[0].Prop, Is.EqualTo("bleed/1"));
            Assert.That(pending[0].Expression.ToString(), Is.EqualTo("1d4"));
            Assert.That(pending[0].ActionName, Is.Null);

            //Nobody settles the roll. Time moves on anyway and the rolls queue up.
            characterSheet.AdvanceToTurn(4);

            Assert.That(characterSheet.Time.Turn, Is.EqualTo(4));
            Assert.That(characterSheet.GetPendingRolls().Select(x => x.Prop),
                Is.EquivalentTo(new[] { "bleed/1", "bleed/2", "bleed/3", "bleed/4" }));
            Assert.That(Roller(characterSheet).Rolled, Is.EqualTo(0));

            //One by the player, the rest by the app
            characterSheet.SetRoll(creature, "bleed/2", 4);
            var rolls = characterSheet.RollPending();

            Assert.That(rolls.Length, Is.EqualTo(3));
            Assert.That(characterSheet.GetPendingRolls(), Is.Empty);
            Assert.That(characterSheet.GetPropertyValue<int>(creature, "bleed/1"), Is.EqualTo(2));
            Assert.That(characterSheet.GetPropertyValue<int>(creature, "bleed/2"), Is.EqualTo(4));
            Assert.That(characterSheet.GetPropertyValue<int>(creature, "bleed/3"), Is.EqualTo(3));
            Assert.That(characterSheet.GetPropertyValue<int>(creature, "bleed/4"), Is.EqualTo(4));

            //Asking again for the same roll does not undo it
            characterSheet.Time.Refresh();
            Assert.That(characterSheet.GetPendingRolls(), Is.Empty);
            Assert.That(characterSheet.GetPropertyValue<int>(creature, "bleed/4"), Is.EqualTo(4));
        }

        #endregion Rolls caused by time
    }
}
