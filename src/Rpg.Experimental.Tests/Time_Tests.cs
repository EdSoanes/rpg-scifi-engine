using Rpg.Experimental.Json;
using Rpg.Experimental.Mods;
using Rpg.Experimental.Reflection;
using Rpg.Experimental.System;
using Rpg.Experimental.Tests.Models;
using Rpg.Experimental.Time;
using Clock = Rpg.Experimental.Time.Temporal;
using Temporal = Rpg.Experimental.Mods.Temporal;

namespace Rpg.Experimental.Tests
{
    /// <summary>
    /// Time keeping as described in feature 5 of docs/VISION.md
    /// </summary>
    public class Time_Tests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeUtilities.RegisterAssembly(typeof(TestObject).Assembly);
        }

        private static RpgActivityAction Strike(RpgCharacterSheet characterSheet, TestCreature creature, int effectTurns = 0, int trivial = 0)
        {
            var activity = characterSheet.CreateActivity(creature.Id, creature.Id, nameof(TestStrike));
            var strike = activity.CurrentActivityAction!;

            Assert.That(strike.Cost(characterSheet), Is.True);
            Assert.That(strike.Perform(characterSheet), Is.True);
            Assert.That(strike.Outcome(characterSheet, ("effectTurns", effectTurns), ("trivial", trivial)), Is.True);

            strike.Complete(characterSheet);
            return strike;
        }

        #region Turn tracking

        [Test]
        public void TurnTracking_NotOnByDefault()
        {
            var creature = new TestCreature();
            var characterSheet = new RpgCharacterSheet(creature);

            Assert.That(characterSheet.Time.IsTurnTracking, Is.False);
            Assert.That(characterSheet.Time.Turn, Is.EqualTo(0));
            Assert.That(characterSheet.GetTurnTrackingReport().IsNeeded, Is.False);
        }

        [Test]
        public void TurnTracking_CanStartAtAnyTurn()
        {
            var creature = new TestCreature();
            var characterSheet = new RpgCharacterSheet(creature);

            characterSheet.BeginTurnTracking(4);

            Assert.That(characterSheet.Time.IsTurnTracking, Is.True);
            Assert.That(characterSheet.Time.Turn, Is.EqualTo(4));

            //Durations are relative to the turn they begin in
            characterSheet.Add(new Temporal(2), creature, x => x.Bonus, 1);
            characterSheet.Time.Refresh();
            Assert.That(creature.Bonus, Is.EqualTo(1));

            characterSheet.NextTurn();
            Assert.That(characterSheet.Time.Turn, Is.EqualTo(5));
            Assert.That(creature.Bonus, Is.EqualTo(1));

            characterSheet.NextTurn();
            Assert.That(creature.Bonus, Is.EqualTo(0));
        }

        [Test]
        public void TurnTracking_BeginWhenAlreadyTracking_DoesNothing()
        {
            var creature = new TestCreature();
            var characterSheet = new RpgCharacterSheet(creature);

            characterSheet.BeginTurnTracking(3);
            characterSheet.BeginTurnTracking();

            Assert.That(characterSheet.Time.Turn, Is.EqualTo(3));
        }

        [Test]
        public void TurnTracking_AdvanceSeveralTurns_PassesThroughEachTurn()
        {
            var temporal = new Clock();
            temporal.BeginTurnTracking();

            var events = new List<TimePoint>();
            temporal.OnTemporalEvent += (sender, e) => events.Add(e.Time);

            temporal.ToTurn(4);

            Assert.That(events.Select(x => x.Count).ToArray(), Is.EqualTo(new[] { 2, 3, 4 }));
            Assert.That(events.All(x => x.Type == TimePointType.Turn), Is.True);
        }

        [Test]
        public void TurnTracking_AdvanceSeveralTurns_SameAsOneAtATime()
        {
            var creature1 = new TestCreature();
            var sheet1 = new RpgCharacterSheet(creature1);
            sheet1.BeginTurnTracking();
            sheet1.Add(new Temporal(1, 2), creature1, x => x.Bonus, 1);
            sheet1.Add(new Temporal(3), creature1, x => x.Health, 1);
            sheet1.Time.Refresh();

            var creature2 = new TestCreature();
            var sheet2 = new RpgCharacterSheet(creature2);
            sheet2.BeginTurnTracking();
            sheet2.Add(new Temporal(1, 2), creature2, x => x.Bonus, 1);
            sheet2.Add(new Temporal(3), creature2, x => x.Health, 1);
            sheet2.Time.Refresh();

            sheet1.AdvanceToTurn(5);

            sheet2.NextTurn();
            sheet2.NextTurn();
            sheet2.NextTurn();
            sheet2.NextTurn();

            Assert.That(sheet1.Time.Turn, Is.EqualTo(sheet2.Time.Turn));
            Assert.That(creature1.Bonus, Is.EqualTo(creature2.Bonus));
            Assert.That(creature1.Health, Is.EqualTo(creature2.Health));
            Assert.That(creature1.Bonus, Is.EqualTo(0));
            Assert.That(creature1.Health, Is.EqualTo(5));
        }

        [Test]
        public void TurnTracking_EffectAppliedOutside_StartsIt()
        {
            var creature = new TestCreature();
            var characterSheet = new RpgCharacterSheet(creature);

            characterSheet.Add(new Temporal(3), creature, x => x.Bonus, 2);
            characterSheet.Time.Refresh();

            Assert.That(characterSheet.Time.IsTurnTracking, Is.True);
            Assert.That(characterSheet.Time.Turn, Is.EqualTo(1));
            Assert.That(creature.Bonus, Is.EqualTo(2));

            characterSheet.AdvanceToTurn(3);
            Assert.That(creature.Bonus, Is.EqualTo(2));

            characterSheet.NextTurn();
            Assert.That(creature.Bonus, Is.EqualTo(0));
        }

        [Test]
        public void TurnTracking_TrivialEffectOutside_DoesNotStartIt()
        {
            var creature = new TestCreature();
            var characterSheet = new RpgCharacterSheet(creature);

            characterSheet.Add(new Temporal(1).NoTurnTracking(), creature, x => x.Bonus, 2);
            characterSheet.Time.Refresh();

            Assert.That(characterSheet.Time.IsTurnTracking, Is.False);
            Assert.That(creature.Bonus, Is.EqualTo(0));

            //It was dropped, it does not lie in wait for the next encounter
            characterSheet.BeginTurnTracking();
            Assert.That(creature.Bonus, Is.EqualTo(0));
        }

        [Test]
        public void TurnTracking_TrivialEffectInside_RunsNormally()
        {
            var creature = new TestCreature();
            var characterSheet = new RpgCharacterSheet(creature);
            characterSheet.BeginTurnTracking();

            characterSheet.Add(new Temporal(1).NoTurnTracking(), creature, x => x.Bonus, 2);
            characterSheet.Time.Refresh();

            Assert.That(creature.Bonus, Is.EqualTo(2));

            characterSheet.NextTurn();
            Assert.That(creature.Bonus, Is.EqualTo(0));
        }

        [Test]
        public void TurnTracking_StateThatNeedsIt_StartsIt_AndStaysOnWhenEnded()
        {
            var creature = new TestCreature();
            var characterSheet = new RpgCharacterSheet(creature);

            Assert.That(creature.IsStateOn(nameof(TestBleeding)), Is.False);

            characterSheet.Add(new Standard(), creature, x => x.Health, -3);
            characterSheet.Time.Refresh();

            Assert.That(creature.Health, Is.EqualTo(2));
            Assert.That(creature.IsStateOn(nameof(TestBleeding)), Is.True);
            Assert.That(characterSheet.Time.IsTurnTracking, Is.True);
            Assert.That(characterSheet.Time.Turn, Is.EqualTo(1));

            var report = characterSheet.GetTurnTrackingReport();
            Assert.That(report.IsNeeded, Is.True);
            Assert.That(report.States.Select(x => x.Name), Is.EqualTo(new[] { nameof(TestBleeding) }));

            //The players can end turn tracking anyway. The creature is still bleeding.
            characterSheet.NextTurn();
            characterSheet.EndTurnTracking();

            Assert.That(characterSheet.Time.IsTurnTracking, Is.False);
            Assert.That(creature.IsStateOn(nameof(TestBleeding)), Is.True);
            Assert.That(characterSheet.GetTurnTrackingReport().IsNeeded, Is.True);

            //...and turn tracking does not start itself again
            characterSheet.Time.Refresh();
            characterSheet.TriggerTimeEvent(Clock.TimePassesEvent);
            Assert.That(characterSheet.Time.IsTurnTracking, Is.False);
        }

        [Test]
        public void TurnTracking_NeverEndsItself_ReportSaysWhenNothingRemains()
        {
            var creature = new TestCreature();
            var characterSheet = new RpgCharacterSheet(creature);

            characterSheet.Add(new Temporal(2), creature, x => x.Bonus, 2);
            characterSheet.Time.Refresh();

            var report = characterSheet.GetTurnTrackingReport();
            Assert.That(report.IsTurnTracking, Is.True);
            Assert.That(report.Turn, Is.EqualTo(1));
            Assert.That(report.IsNeeded, Is.True);
            Assert.That(report.Effects.Length, Is.EqualTo(1));

            characterSheet.AdvanceToTurn(3);

            report = characterSheet.GetTurnTrackingReport();
            Assert.That(creature.Bonus, Is.EqualTo(0));
            Assert.That(report.IsNeeded, Is.False);
            Assert.That(report.IsTurnTracking, Is.True);

            characterSheet.AdvanceToTurn(10);
            Assert.That(characterSheet.Time.IsTurnTracking, Is.True);
        }

        [Test]
        public void TurnTracking_Ended_RunningEffectsAreDropped()
        {
            var creature = new TestCreature();
            var characterSheet = new RpgCharacterSheet(creature);
            characterSheet.BeginTurnTracking();

            characterSheet.Add(new Temporal(10), creature, x => x.Bonus, 2);
            characterSheet.Add(new Temporal(2, 3), creature, x => x.Health, 1);
            characterSheet.Add(new Standard(), creature, x => x.Ammo, -4);
            characterSheet.Time.Refresh();

            Assert.That(creature.Bonus, Is.EqualTo(2));
            Assert.That(characterSheet.GetTurnTrackingReport().Effects.Length, Is.EqualTo(2));

            characterSheet.EndTurnTracking();

            Assert.That(characterSheet.Time.IsTurnTracking, Is.False);
            Assert.That(creature.Bonus, Is.EqualTo(0));
            Assert.That(creature.Health, Is.EqualTo(5));
            Assert.That(characterSheet.GetTurnTrackingReport().IsNeeded, Is.False);

            //Lasting changes carry on
            Assert.That(creature.Ammo, Is.EqualTo(6));

            //Nothing dropped comes back in the next encounter
            characterSheet.BeginTurnTracking();
            characterSheet.AdvanceToTurn(3);

            Assert.That(creature.Bonus, Is.EqualTo(0));
            Assert.That(creature.Health, Is.EqualTo(5));
            Assert.That(creature.Ammo, Is.EqualTo(6));
        }

        #endregion Turn tracking

        #region Renumbering

        [Test]
        public void Renumber_NoTimePasses_EffectsKeepTheirRemainingTurns()
        {
            var creature = new TestCreature();
            var characterSheet = new RpgCharacterSheet(creature);
            characterSheet.BeginTurnTracking();
            characterSheet.NextTurn();

            //Active on turns 2 and 3
            characterSheet.Add(new Temporal(2), creature, x => x.Bonus, 2);
            //Active on turns 3 and 4
            characterSheet.Add(new Temporal(1, 2), creature, x => x.Health, 1);
            characterSheet.Time.Refresh();

            Assert.That(creature.Bonus, Is.EqualTo(2));
            Assert.That(creature.Health, Is.EqualTo(5));

            characterSheet.RenumberTurn(7);

            Assert.That(characterSheet.Time.Turn, Is.EqualTo(7));
            Assert.That(creature.Bonus, Is.EqualTo(2));
            Assert.That(creature.Health, Is.EqualTo(5));

            characterSheet.NextTurn();
            Assert.That(characterSheet.Time.Turn, Is.EqualTo(8));
            Assert.That(creature.Bonus, Is.EqualTo(2));
            Assert.That(creature.Health, Is.EqualTo(6));

            characterSheet.NextTurn();
            Assert.That(creature.Bonus, Is.EqualTo(0));
            Assert.That(creature.Health, Is.EqualTo(6));

            characterSheet.NextTurn();
            Assert.That(creature.Health, Is.EqualTo(5));
        }

        [Test]
        public void Renumber_Backwards_Works()
        {
            var creature = new TestCreature();
            var characterSheet = new RpgCharacterSheet(creature);
            characterSheet.BeginTurnTracking(5);

            characterSheet.Add(new Temporal(2), creature, x => x.Bonus, 2);
            characterSheet.Time.Refresh();

            characterSheet.RenumberTurn(2);

            Assert.That(characterSheet.Time.Turn, Is.EqualTo(2));
            Assert.That(creature.Bonus, Is.EqualTo(2));

            characterSheet.AdvanceToTurn(4);
            Assert.That(creature.Bonus, Is.EqualTo(0));
        }

        [Test]
        public void Renumber_OutsideTurnTracking_DoesNothing()
        {
            var creature = new TestCreature();
            var characterSheet = new RpgCharacterSheet(creature);

            characterSheet.RenumberTurn(4);

            Assert.That(characterSheet.Time.IsTurnTracking, Is.False);
        }

        #endregion Renumbering

        #region Time events

        [Test]
        public void System_DeclaresItsTimeEvents()
        {
            var system = RpgSystemFactory.Build(new TestSystem());

            Assert.That(system.TimeEvents, Is.EqualTo(new[] { "Sunrise", "Sunset" }));
        }

        [Test]
        public void TimeEvent_UntilEvent_EndsOnThatEventOnly()
        {
            var creature = new TestCreature();
            var characterSheet = new RpgCharacterSheet(creature);

            characterSheet.Add(new Standard().Until("Sunrise"), creature, x => x.Bonus, 3);
            characterSheet.Time.Refresh();

            Assert.That(characterSheet.Time.IsTurnTracking, Is.False);
            Assert.That(creature.Bonus, Is.EqualTo(3));

            characterSheet.TriggerTimeEvent("Sunset");
            Assert.That(creature.Bonus, Is.EqualTo(3));
            Assert.That(characterSheet.Time.LastEvent, Is.EqualTo("Sunset"));

            characterSheet.TriggerTimeEvent(Clock.TimePassesEvent);
            Assert.That(creature.Bonus, Is.EqualTo(3));

            characterSheet.TriggerTimeEvent("Sunrise");
            Assert.That(creature.Bonus, Is.EqualTo(0));
            Assert.That(characterSheet.Time.LastEvent, Is.EqualTo("Sunrise"));
            Assert.That(characterSheet.Time.EventSequence, Is.EqualTo(3));
        }

        [Test]
        public void TimeEvent_StartAtEvent_PendingUntilThen()
        {
            var creature = new TestCreature();
            var characterSheet = new RpgCharacterSheet(creature);

            var mod = new Standard()
                .Lifespan(TimePoint.AtEvent("Sunset"), TimePoint.AtEvent("Sunrise"));

            characterSheet.Add(mod, creature, x => x.Bonus, 3);
            characterSheet.Time.Refresh();

            Assert.That(creature.Bonus, Is.EqualTo(0));

            characterSheet.TriggerTimeEvent("Sunrise");
            Assert.That(creature.Bonus, Is.EqualTo(0));

            characterSheet.TriggerTimeEvent("Sunset");
            Assert.That(creature.Bonus, Is.EqualTo(3));

            characterSheet.TriggerTimeEvent("Sunrise");
            Assert.That(creature.Bonus, Is.EqualTo(0));
        }

        [Test]
        public void TimeEvent_NamedEvent_AlsoCountsAsTimePassing()
        {
            var creature = new TestCreature();
            var characterSheet = new RpgCharacterSheet(creature);

            var mod = new Standard()
                .Lifespan(TimePointType.TimeBegins, TimePointType.TimePasses);

            characterSheet.Add(mod, creature, x => x.Bonus, 3);
            characterSheet.Time.Refresh();

            Assert.That(creature.Bonus, Is.EqualTo(3));

            characterSheet.TriggerTimeEvent("Sunset");
            Assert.That(creature.Bonus, Is.EqualTo(0));
        }

        [Test]
        public void TimeEvent_UntilTimePasses_NotEndedByCountingTurns()
        {
            var creature = new TestCreature();
            var characterSheet = new RpgCharacterSheet(creature);

            //A fuzzy duration: for as long as something is going on
            var mod = new Standard()
                .Lifespan(TimePointType.TimeBegins, TimePointType.TimePasses);

            characterSheet.Add(mod, creature, x => x.Bonus, 3);
            characterSheet.Time.Refresh();
            Assert.That(creature.Bonus, Is.EqualTo(3));

            //Turns and time events are separate clocks
            characterSheet.BeginTurnTracking();
            Assert.That(creature.Bonus, Is.EqualTo(3));

            characterSheet.AdvanceToTurn(3);
            Assert.That(creature.Bonus, Is.EqualTo(3));
            Assert.That(creature.EventsSeen, Is.EqualTo(string.Empty));

            characterSheet.EndTurnTracking();
            Assert.That(creature.Bonus, Is.EqualTo(3));
            Assert.That(characterSheet.Time.EventSequence, Is.EqualTo(0));

            //The story moves on
            characterSheet.TriggerTimeEvent(Clock.TimePassesEvent);
            Assert.That(creature.Bonus, Is.EqualTo(0));
            Assert.That(characterSheet.Time.EventSequence, Is.EqualTo(1));
        }

        [Test]
        public void TimeEvent_UndeclaredEvent_IsAccepted()
        {
            var creature = new TestCreature();
            var characterSheet = new RpgCharacterSheet(creature);

            characterSheet.Add(new Standard().Until("FullMoon"), creature, x => x.Bonus, 3);
            characterSheet.Time.Refresh();
            Assert.That(creature.Bonus, Is.EqualTo(3));

            characterSheet.TriggerTimeEvent("FullMoon");
            Assert.That(creature.Bonus, Is.EqualTo(0));
        }

        [Test]
        public void TimeEvent_RulesCanReact()
        {
            var creature = new TestCreature();
            var characterSheet = new RpgCharacterSheet(creature);

            Assert.That(creature.EventsSeen, Is.EqualTo(string.Empty));

            characterSheet.TriggerTimeEvent("Sunrise");
            characterSheet.BeginTurnTracking();
            characterSheet.NextTurn();
            characterSheet.TriggerTimeEvent("Sunset");
            characterSheet.TriggerTimeEvent(Clock.TimePassesEvent);

            Assert.That(creature.EventsSeen, Is.EqualTo("Sunrise;Sunset;TimePasses;"));
        }

        [Test]
        public void TimeEvent_DuringTurnTracking_DoesNotEndIt()
        {
            var creature = new TestCreature();
            var characterSheet = new RpgCharacterSheet(creature);

            characterSheet.Add(new Standard().Until("Sunrise"), creature, x => x.Bonus, 3);
            characterSheet.BeginTurnTracking();
            characterSheet.Add(new Temporal(5), creature, x => x.Health, 1);
            characterSheet.AdvanceToTurn(2);

            Assert.That(creature.Bonus, Is.EqualTo(3));
            Assert.That(creature.Health, Is.EqualTo(6));

            characterSheet.TriggerTimeEvent("Sunrise");

            Assert.That(characterSheet.Time.IsTurnTracking, Is.True);
            Assert.That(characterSheet.Time.Turn, Is.EqualTo(2));
            Assert.That(creature.Bonus, Is.EqualTo(0));
            Assert.That(creature.Health, Is.EqualTo(6));

            characterSheet.TriggerTimeEvent(Clock.TimePassesEvent);

            Assert.That(characterSheet.Time.IsTurnTracking, Is.True);
            Assert.That(characterSheet.Time.Turn, Is.EqualTo(2));
            Assert.That(creature.Health, Is.EqualTo(6));

            //The ended effect stays ended after turn tracking
            characterSheet.EndTurnTracking();
            Assert.That(creature.Bonus, Is.EqualTo(0));
        }

        [Test]
        public void TimeEvent_UntilEvent_BegunDuringTurnTracking_OutlivesIt()
        {
            var creature = new TestCreature();
            var characterSheet = new RpgCharacterSheet(creature);
            characterSheet.BeginTurnTracking();
            characterSheet.AdvanceToTurn(3);

            //From this turn until sunrise
            var mod = new Standard()
                .Lifespan(characterSheet.Time.Now, TimePoint.AtEvent("Sunrise"));

            characterSheet.Add(mod, creature, x => x.Bonus, 3);
            characterSheet.Time.Refresh();
            Assert.That(creature.Bonus, Is.EqualTo(3));

            characterSheet.AdvanceToTurn(6);
            Assert.That(creature.Bonus, Is.EqualTo(3));

            characterSheet.EndTurnTracking();
            Assert.That(creature.Bonus, Is.EqualTo(3));

            characterSheet.BeginTurnTracking();
            Assert.That(creature.Bonus, Is.EqualTo(3));
            characterSheet.EndTurnTracking();

            characterSheet.TriggerTimeEvent("Sunrise");
            Assert.That(creature.Bonus, Is.EqualTo(0));
        }

        [Test]
        public void TimeEvent_ModSetUntilEvent_AppliesAndEnds()
        {
            var creature = new TestCreature();
            var characterSheet = new RpgCharacterSheet(creature);

            var modSet = new RpgModSet().Until("Sunrise");
            modSet.Add(creature, x => x.Bonus, 2);
            modSet.Add(creature, x => x.Health, 1);

            characterSheet.Add(modSet);
            characterSheet.Time.Refresh();

            Assert.That(creature.Bonus, Is.EqualTo(2));
            Assert.That(creature.Health, Is.EqualTo(6));

            characterSheet.TriggerTimeEvent("Sunrise");

            Assert.That(creature.Bonus, Is.EqualTo(0));
            Assert.That(creature.Health, Is.EqualTo(5));
            Assert.That(characterSheet.ObjectExists(modSet.Id), Is.False);
        }

        [Test]
        public void TimeEvent_SurvivesSaveAndRestore()
        {
            var creature = new TestCreature();
            var characterSheet = new RpgCharacterSheet(creature);

            characterSheet.Add(new Standard().Until("Sunrise"), creature, x => x.Bonus, 3);
            characterSheet.TriggerTimeEvent("Sunset");

            var json = RpgJson.SerializeGraphState(characterSheet.GetState());
            var state = RpgJson.DeserializeGraphState<RpgCharacterSheetState>(json);
            var characterSheet2 = new RpgCharacterSheet(state, characterSheet.GetSystem());
            var creature2 = (characterSheet2.GetObject(creature.Id) as TestCreature)!;

            Assert.That(characterSheet2.Time.LastEvent, Is.EqualTo("Sunset"));
            Assert.That(characterSheet2.Time.EventSequence, Is.EqualTo(1));
            Assert.That(creature2.Bonus, Is.EqualTo(3));

            characterSheet2.TriggerTimeEvent("Sunrise");
            Assert.That(creature2.Bonus, Is.EqualTo(0));
        }

        #endregion Time events

        #region Costs and effects

        [Test]
        public void Cost_OutsideTurnTracking_TurnCostSkipped_PermanentCostApplied()
        {
            var creature = new TestCreature();
            var characterSheet = new RpgCharacterSheet(creature);

            var strike = Strike(characterSheet, creature);

            Assert.That(strike.IsComplete, Is.True);
            Assert.That(characterSheet.Time.IsTurnTracking, Is.False);

            Assert.That(creature.Actions, Is.EqualTo(1));
            Assert.That(creature.Ammo, Is.EqualTo(9));

            //The skipped cost is kept so it can be shown
            Assert.That(strike.SkippedCosts.Count, Is.EqualTo(1));
            Assert.That(strike.SkippedCosts[0].Target?.Path, Is.EqualTo(nameof(TestCreature.Actions)));
            Assert.That(strike.CostSet.Mods.Length, Is.EqualTo(1));
        }

        [Test]
        public void Cost_OutsideTurnTracking_SkippedCostCanBeAppliedByHand()
        {
            var creature = new TestCreature();
            var characterSheet = new RpgCharacterSheet(creature);

            var strike = Strike(characterSheet, creature);
            Assert.That(creature.Actions, Is.EqualTo(1));

            strike.ApplySkippedCosts(characterSheet);

            Assert.That(strike.SkippedCosts.Count, Is.EqualTo(0));
            Assert.That(characterSheet.Time.IsTurnTracking, Is.True);
            Assert.That(creature.Actions, Is.EqualTo(0));

            characterSheet.NextTurn();
            Assert.That(creature.Actions, Is.EqualTo(1));
            Assert.That(creature.Ammo, Is.EqualTo(9));
        }

        [Test]
        public void Cost_EffectStartsTurnTracking_TurnCostApplied()
        {
            var creature = new TestCreature();
            var characterSheet = new RpgCharacterSheet(creature);

            var strike = Strike(characterSheet, creature, effectTurns: 2);

            //The effect lasts 2 turns so the clock is now running, and this turn's action is spent
            Assert.That(characterSheet.Time.IsTurnTracking, Is.True);
            Assert.That(characterSheet.Time.Turn, Is.EqualTo(1));
            Assert.That(creature.Bonus, Is.EqualTo(2));
            Assert.That(creature.Actions, Is.EqualTo(0));
            Assert.That(creature.Ammo, Is.EqualTo(9));
            Assert.That(strike.SkippedCosts.Count, Is.EqualTo(0));

            characterSheet.NextTurn();
            Assert.That(creature.Bonus, Is.EqualTo(2));
            Assert.That(creature.Actions, Is.EqualTo(1));

            characterSheet.NextTurn();
            Assert.That(creature.Bonus, Is.EqualTo(0));
            Assert.That(characterSheet.GetTurnTrackingReport().IsNeeded, Is.False);

            characterSheet.EndTurnTracking();
            Assert.That(creature.Ammo, Is.EqualTo(9));
        }

        [Test]
        public void Cost_TrivialEffectOutsideTurnTracking_NothingStarts_CostSkipped()
        {
            var creature = new TestCreature();
            var characterSheet = new RpgCharacterSheet(creature);

            var strike = Strike(characterSheet, creature, effectTurns: 1, trivial: 1);

            Assert.That(characterSheet.Time.IsTurnTracking, Is.False);
            Assert.That(creature.Bonus, Is.EqualTo(0));
            Assert.That(creature.Actions, Is.EqualTo(1));
            Assert.That(creature.Ammo, Is.EqualTo(9));
            Assert.That(strike.SkippedCosts.Count, Is.EqualTo(1));
        }

        [Test]
        public void Cost_InsideTurnTracking_AllCostsApplied()
        {
            var creature = new TestCreature();
            var characterSheet = new RpgCharacterSheet(creature);
            characterSheet.BeginTurnTracking();

            var strike = Strike(characterSheet, creature, effectTurns: 1, trivial: 1);

            Assert.That(creature.Bonus, Is.EqualTo(2));
            Assert.That(creature.Actions, Is.EqualTo(0));
            Assert.That(creature.Ammo, Is.EqualTo(9));
            Assert.That(strike.SkippedCosts.Count, Is.EqualTo(0));

            characterSheet.NextTurn();

            Assert.That(creature.Bonus, Is.EqualTo(0));
            Assert.That(creature.Actions, Is.EqualTo(1));
            Assert.That(creature.Ammo, Is.EqualTo(9));
        }

        [Test]
        public void Cost_NothingAppliedBeforeComplete()
        {
            var creature = new TestCreature();
            var characterSheet = new RpgCharacterSheet(creature);

            var activity = characterSheet.CreateActivity(creature.Id, creature.Id, nameof(TestStrike));
            var strike = activity.CurrentActivityAction!;

            Assert.That(strike.Cost(characterSheet), Is.True);
            Assert.That(strike.Outcome(characterSheet, ("effectTurns", 3), ("trivial", 0)), Is.True);

            Assert.That(strike.CostSet.Mods.Length, Is.EqualTo(2));
            Assert.That(strike.Result.Mods.Length, Is.EqualTo(1));
            Assert.That(characterSheet.Time.IsTurnTracking, Is.False);
            Assert.That(creature.Actions, Is.EqualTo(1));
            Assert.That(creature.Ammo, Is.EqualTo(10));
            Assert.That(creature.Bonus, Is.EqualTo(0));
        }

        [Test]
        public void ResetStep_RemovesWhatTheStepAdded()
        {
            var creature = new TestCreature();
            var characterSheet = new RpgCharacterSheet(creature);
            characterSheet.BeginTurnTracking();

            var activity = characterSheet.CreateActivity(creature.Id, creature.Id, nameof(TestStrike));
            var strike = activity.CurrentActivityAction!;

            Assert.That(strike.Cost(characterSheet), Is.True);
            Assert.That(strike.Outcome(characterSheet, ("effectTurns", 3), ("trivial", 0)), Is.True);
            Assert.That(strike.Result.Mods.Length, Is.EqualTo(1));

            //Redo the outcome with a different value
            strike.Reset(characterSheet, ActionMethodNames.Outcome);

            Assert.That(strike.Result.Mods.Length, Is.EqualTo(0));
            Assert.That(strike.CostSet.Mods.Length, Is.EqualTo(2));
            Assert.That(strike.OutcomeMethod.IsDone, Is.False);

            Assert.That(strike.Outcome(characterSheet, ("effectTurns", 0)), Is.True);
            Assert.That(strike.Result.Mods.Length, Is.EqualTo(0));

            //Redo everything
            strike.Reset(characterSheet, ActionMethodNames.Cost);

            Assert.That(strike.CostSet.Mods.Length, Is.EqualTo(0));
            Assert.That(strike.CostMethod.IsDone, Is.False);

            Assert.That(strike.Cost(characterSheet), Is.True);
            Assert.That(strike.Outcome(characterSheet), Is.True);
            strike.Complete(characterSheet);

            Assert.That(creature.Actions, Is.EqualTo(0));
            Assert.That(creature.Ammo, Is.EqualTo(9));
            Assert.That(creature.Bonus, Is.EqualTo(0));
        }

        [Test]
        public void ResetAction_AfterComplete_UndoesIt()
        {
            var creature = new TestCreature();
            var characterSheet = new RpgCharacterSheet(creature);
            characterSheet.BeginTurnTracking();

            var strike = Strike(characterSheet, creature, effectTurns: 2);

            Assert.That(creature.Actions, Is.EqualTo(0));
            Assert.That(creature.Ammo, Is.EqualTo(9));
            Assert.That(creature.Bonus, Is.EqualTo(2));

            strike.Reset(characterSheet);

            Assert.That(strike.IsComplete, Is.False);
            Assert.That(creature.Actions, Is.EqualTo(1));
            Assert.That(creature.Ammo, Is.EqualTo(10));
            Assert.That(creature.Bonus, Is.EqualTo(0));
        }

        #endregion Costs and effects
    }
}
