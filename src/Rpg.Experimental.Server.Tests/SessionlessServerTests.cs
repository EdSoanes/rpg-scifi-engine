using Rpg.Cyborgs;
using Rpg.Cyborgs.Actions;
using Rpg.Cyborgs.States;
using Rpg.Experimental.Description;

namespace Rpg.Experimental.Server.Tests
{
    /// <summary>
    /// The operations a character sheet user interface performs, sent as json the way a web client sends
    /// them. Nothing is kept between requests: the sheet travels with each one.
    /// </summary>
    public class SessionlessServerTests
    {
        private const string System = TestServer.System;

        private RpgSessionlessServer _server;
        private RpgSystems _systems;
        private string _sheet;
        private RpgSheetView _view;

        [SetUp]
        public void Setup()
        {
            (_server, _systems) = TestServer.Create();

            var created = _server.CreateSheet(System, nameof(PlayerCharacter), TestContentFactory.BennyKey.ToString());
            _sheet = created.Sheet;
            _view = created.Data;
        }

        private RpgObjectView Benny => _view.Objects.Single(x => x.Description.ObjectId == _view.ActorId);
        private RpgObjectView Sword => _view.Objects.Single(x => x.Description.Name == "Excalibur");

        private RpgSheetView Refresh()
        {
            var response = TestServer.Send<object?, RpgSheetView>(_sheet, null, r => _server.GetSheet(System, r));
            _view = response.Data;
            return _view;
        }

        private Dice Value(RpgObjectView obj, string prop)
            => obj.Description.Properties.Single(x => x.Prop == prop).Value;

        private RpgActionResult Do(RpgResponse<RpgActionResult> response)
        {
            _sheet = response.Sheet;
            return response.Data;
        }

        private RpgActionResult Initiate(string actionOwnerId, string actionName)
            => Do(TestServer.Send<InitiateAction, RpgActionResult>(_sheet,
                new InitiateAction { InitiatorId = _view.ActorId, ActionOwnerId = actionOwnerId, ActionName = actionName },
                r => _server.InitiateAction(System, r)));

        private RpgActionResult Step(string activityActionId, string step, Dictionary<string, object?>? args = null)
            => Do(TestServer.Send<ActionStepRun, RpgActionResult>(_sheet,
                new ActionStepRun { ActivityActionId = activityActionId, Step = step, Args = args ?? new() },
                r => _server.ActionStep(System, r)));

        private RpgTimeView Time(TimeOpKind kind, int? turn = null, string? eventName = null)
        {
            var response = TestServer.Send<TimeOp, RpgTimeView>(_sheet,
                new TimeOp { Kind = kind, Turn = turn, Event = eventName },
                r => _server.Time(System, r));

            _sheet = response.Sheet;
            return response.Data;
        }

        #region Systems and content

        [Test]
        public void Systems_AreRegisteredAndListed()
        {
            Assert.That(_server.ListSystems().Select(x => x.Identifier), Is.EqualTo(new[] { "Cyborgs" }));
            Assert.That(_server.GetSystem("cyborgs").Name, Is.EqualTo("Cyborgs & Sidearms"));
            Assert.Throws<RpgServerException>(() => _server.GetSystem("Nope"));
        }

        [Test]
        public void Content_IsListed()
        {
            var entities = _server.ListEntities(System);

            Assert.That(entities.Length, Is.EqualTo(1));
            Assert.That(entities[0].Name, Is.EqualTo("Benny"));
            Assert.That(entities[0].Archetype, Is.EqualTo(nameof(PlayerCharacter)));
        }

        [Test]
        public void CreateSheet_GivesASheetAndAViewOfIt()
        {
            Assert.That(_sheet, Is.Not.Empty);
            Assert.That(_sheet.TrimStart().StartsWith('{'), Is.False, "the sheet travels compressed");

            Assert.That(_view.System, Is.EqualTo("Cyborgs"));
            Assert.That(_view.RollMode, Is.EqualTo(RpgRollMode.Ask));
            Assert.That(_view.MaxTurnHistory, Is.EqualTo(5));
            Assert.That(_view.Time.IsTurnTracking, Is.False);
            Assert.That(_view.Time.TimeEvents, Is.EqualTo(new[] { "Sunrise", "Sunset", "TimePasses" }));
            Assert.That(_view.PendingRolls, Is.Empty);
            Assert.That(_view.ActionsInProgress, Is.Empty);

            //The character, its body parts and its gear. Not actions or other machinery.
            Assert.That(_view.Objects.Select(x => x.Description.Name),
                Is.EquivalentTo(new[] { "Benny", "Excalibur", "Vest", "Head", "Torso", "LeftArm", "RightArm", "LeftLeg", "RightLeg" }));

            Assert.That(Benny.Description.Archetype, Is.EqualTo(nameof(PlayerCharacter)));
            Assert.That(Value(Benny, "Defence"), Is.EqualTo(new Dice(7)));
            Assert.That(Value(Benny, "StaminaPoints"), Is.EqualTo(new Dice(14)));
            Assert.That(Value(Sword, "Damage"), Is.EqualTo(new Dice("1d6")));

            Assert.That(Benny.Children["Hands"], Is.EqualTo(new[] { Sword.Description.ObjectId }));
            Assert.That(Benny.Children["Wearing"].Length, Is.EqualTo(1));

            Assert.That(Sword.Actions.Select(x => x.Name), Is.EqualTo(new[] { "MeleeAttack", "Transfer" }));
            Assert.That(Sword.Actions.Single(x => x.Name == "MeleeAttack").IsPerformable, Is.True);
        }

        [Test]
        public void CreateSheet_UnknownContent_IsAnError()
        {
            Assert.Throws<RpgServerException>(() => _server.CreateSheet(System, nameof(PlayerCharacter), Guid.NewGuid().ToString()));
            Assert.Throws<RpgServerException>(() => _server.CreateSheet("Nope", nameof(PlayerCharacter), TestContentFactory.BennyKey.ToString()));
        }

        [Test]
        public void Request_WithoutASheet_IsAnError()
        {
            Assert.Throws<RpgServerException>(() => _server.GetSheet(System, new RpgRequest<object?> { Sheet = "" }));
            Assert.Throws<RpgServerException>(() => _server.GetSheet(System, new RpgRequest<object?> { Sheet = "not a sheet" }));
        }

        #endregion Systems and content

        #region Actions and rolls

        [Test]
        public void Attack_StepByStep_WithRealDice()
        {
            Time(TimeOpKind.BeginTurnTracking);

            var started = Initiate(Sword.Description.ObjectId, nameof(MeleeAttack));
            var id = started.Action.ActivityActionId;

            Assert.That(started.Action.ActionName, Is.EqualTo(nameof(MeleeAttack)));
            Assert.That(started.Action.IsComplete, Is.False);

            //Values arrive from json as loosely typed numbers
            var cost = Step(id, "Cost", new() { { "actionPoints", 1 }, { "focusPoints", 0 } });
            Assert.That(cost.Ran, Is.True);

            var perform = Step(id, "Perform", new() { { "targetDefence", 12 } });
            Assert.That(perform.Ran, Is.True);

            //No roll yet, so the outcome does not run and says what it is waiting for
            var waiting = Step(id, "Outcome");
            Assert.That(waiting.Ran, Is.False);
            Assert.That(waiting.Action.PendingRolls.Single().Prop, Is.EqualTo("diceRoll"));
            Assert.That(waiting.Action.PendingRolls.Single().Expression, Is.EqualTo(new Dice("2d6 + 1")));

            Assert.That(Refresh().PendingRolls.Count, Is.EqualTo(1));
            Assert.That(_view.ActionsInProgress.Single().ActionName, Is.EqualTo(nameof(MeleeAttack)));

            //The player rolls 9 and says so
            var rolled = TestServer.Send<RollOp, RpgPendingRoll[]>(_sheet,
                new RollOp { Kind = RollOpKind.Set, ObjectId = id, Prop = "diceRoll", Result = 9 },
                r => _server.Roll(System, r));

            _sheet = rolled.Sheet;
            Assert.That(rolled.Data, Is.Empty);

            var outcome = Step(id, "Outcome");
            Assert.That(outcome.Ran, Is.True);
            Assert.That(outcome.Action.Rolls.Single().Roll.Result, Is.EqualTo(9));
            Assert.That(outcome.Action.Rolls.Single().Roll.SuppliedBy, Is.EqualTo(RpgRollSource.Player));

            var completed = Do(TestServer.Send<ActionComplete, RpgActionResult>(_sheet,
                new ActionComplete { ActivityActionId = id },
                r => _server.ActionComplete(System, r)));

            Assert.That(completed.Ran, Is.True);
            Assert.That(completed.Action.IsComplete, Is.True);
            Assert.That(completed.Action.Costs.Count, Is.EqualTo(1));
            Assert.That(completed.Action.Effects.Single().IsStateActivation, Is.True);

            Refresh();
            Assert.That(Value(Benny, "CurrentActionPoints"), Is.EqualTo(Dice.Zero));
            Assert.That(Benny.Description.StatesOn, Does.Contain(nameof(MeleeAttacking)));
            Assert.That(_view.ActionsInProgress, Is.Empty);
        }

        [Test]
        public void Attack_NumberSuppliedWithTheStep_IsTheDice()
        {
            Time(TimeOpKind.BeginTurnTracking);

            var id = Initiate(Sword.Description.ObjectId, nameof(MeleeAttack)).Action.ActivityActionId;
            Step(id, "Cost", new() { { "actionPoints", 1 }, { "focusPoints", 0 } });
            Step(id, "Perform", new() { { "targetDefence", 12 } });

            var outcome = Step(id, "Outcome", new() { { "diceRoll", 9 } });

            Assert.That(outcome.Ran, Is.True);

            var input = outcome.Action.Steps.Single(x => x.Name == "Outcome").Inputs.Single(x => x.Name == "diceRoll");
            Assert.That(input.Value, Is.EqualTo("10"));
        }

        [Test]
        public void Attack_AutoComplete_AppRolls()
        {
            Time(TimeOpKind.BeginTurnTracking);

            var id = Initiate(Sword.Description.ObjectId, nameof(MeleeAttack)).Action.ActivityActionId;

            var done = Do(TestServer.Send<ActionAutoComplete, RpgActionResult>(_sheet,
                new ActionAutoComplete { ActivityActionId = id, Args = new() { { "actionPoints", 1 }, { "focusPoints", 0 }, { "targetDefence", 12 } } },
                r => _server.ActionAutoComplete(System, r)));

            Assert.That(done.Ran, Is.True);
            Assert.That(done.Action.IsComplete, Is.True);

            var roll = done.Action.Rolls.Single().Roll;
            Assert.That(roll.SuppliedBy, Is.EqualTo(RpgRollSource.App));
            Assert.That(roll.Result, Is.InRange(2, 12));
        }

        [Test]
        public void Parry_FollowOnActions_ContinueTheActivity()
        {
            _sheet = TestServer.Send<SheetSettings, RpgSheetView>(_sheet,
                new SheetSettings { RollMode = RpgRollMode.App },
                r => _server.Settings(System, r)).Sheet;

            Time(TimeOpKind.BeginTurnTracking);

            var id = Initiate(_view.ActorId, nameof(MeleeParry)).Action.ActivityActionId;
            Step(id, "Cost", new() { { "focusPoints", 0 }, { "damage", 10 } });
            Step(id, "Perform", new() { { "parryTarget", 2 } });
            Assert.That(Step(id, "Outcome").Ran, Is.True);

            var completed = Do(TestServer.Send<ActionComplete, RpgActionResult>(_sheet,
                new ActionComplete { ActivityActionId = id },
                r => _server.ActionComplete(System, r)));

            Assert.That(completed.FollowOnActions.Select(x => x.ActionName), Is.EqualTo(new[] { nameof(ArmourCheck), nameof(TakeDamage) }));

            //Taking the damage continues the same activity, so it knows the damage left after the parry
            var takeDamage = Initiate(completed.FollowOnActions[1].ActionOwnerId, completed.FollowOnActions[1].ActionName);
            var damage = takeDamage.Action.Steps.Single(x => x.Name == "Outcome").Inputs.Single(x => x.Name == "damage");

            Assert.That(damage.Value, Is.EqualTo("9"));
        }

        [Test]
        public void Action_Reset_UndoesIt()
        {
            Time(TimeOpKind.BeginTurnTracking);

            var id = Initiate(Sword.Description.ObjectId, nameof(MeleeAttack)).Action.ActivityActionId;
            Do(TestServer.Send<ActionAutoComplete, RpgActionResult>(_sheet,
                new ActionAutoComplete { ActivityActionId = id, Args = new() { { "actionPoints", 1 }, { "focusPoints", 0 }, { "targetDefence", 12 } } },
                r => _server.ActionAutoComplete(System, r)));

            Refresh();
            Assert.That(Value(Benny, "CurrentActionPoints"), Is.EqualTo(Dice.Zero));

            var reset = Do(TestServer.Send<ActionReset, RpgActionResult>(_sheet,
                new ActionReset { ActivityActionId = id },
                r => _server.ActionReset(System, r)));

            Assert.That(reset.Action.IsComplete, Is.False);
            Assert.That(reset.Action.Rolls, Is.Empty);

            Refresh();
            Assert.That(Value(Benny, "CurrentActionPoints"), Is.EqualTo(new Dice(1)));
        }

        [Test]
        public void Action_UnknownThings_AreErrors()
        {
            Assert.Throws<RpgServerException>(() => _server.InitiateAction(System, new RpgRequest<InitiateAction>
            {
                Sheet = _sheet,
                Op = new InitiateAction { InitiatorId = _view.ActorId, ActionOwnerId = Sword.Description.ObjectId, ActionName = "Juggle" }
            }));

            Assert.Throws<RpgServerException>(() => _server.ActionStep(System, new RpgRequest<ActionStepRun>
            {
                Sheet = _sheet,
                Op = new ActionStepRun { ActivityActionId = "nope", Step = "Cost" }
            }));

            var id = Initiate(Sword.Description.ObjectId, nameof(MeleeAttack)).Action.ActivityActionId;

            Assert.Throws<RpgServerException>(() => _server.ActionStep(System, new RpgRequest<ActionStepRun>
            {
                Sheet = _sheet,
                Op = new ActionStepRun { ActivityActionId = id, Step = "Dance" }
            }));
        }

        #endregion Actions and rolls

        #region Time

        [Test]
        public void Time_TurnTrackingAndGoingBack()
        {
            var time = Time(TimeOpKind.BeginTurnTracking, 3);
            Assert.That(time.IsTurnTracking, Is.True);
            Assert.That(time.Turn, Is.EqualTo(3));

            time = Time(TimeOpKind.NextTurn);
            Assert.That(time.Turn, Is.EqualTo(4));

            time = Time(TimeOpKind.AdvanceToTurn, 6);
            Assert.That(time.RewindableTurns, Is.EqualTo(new[] { 3, 4, 5, 6 }));

            time = Time(TimeOpKind.RenumberTurn, 10);
            Assert.That(time.Turn, Is.EqualTo(10));
            Assert.That(time.RewindableTurns, Is.EqualTo(new[] { 7, 8, 9, 10 }));

            time = Time(TimeOpKind.RewindToTurn, 8);
            Assert.That(time.Turn, Is.EqualTo(8));
            Assert.That(time.RewindableTurns, Is.EqualTo(new[] { 7, 8 }));

            Assert.Throws<RpgServerException>(() => _server.Time(System, new RpgRequest<TimeOp>
            {
                Sheet = _sheet,
                Op = new TimeOp { Kind = TimeOpKind.RewindToTurn, Turn = 2 }
            }));

            Assert.Throws<RpgServerException>(() => _server.Time(System, new RpgRequest<TimeOp>
            {
                Sheet = _sheet,
                Op = new TimeOp { Kind = TimeOpKind.AdvanceToTurn }
            }));

            time = Time(TimeOpKind.EndTurnTracking);
            Assert.That(time.IsTurnTracking, Is.False);
            Assert.That(time.RewindableTurns, Is.Empty);
        }

        [Test]
        public void Time_Events()
        {
            var time = Time(TimeOpKind.TimeEvent, eventName: "Sunset");
            Assert.That(time.LastEvent, Is.EqualTo("Sunset"));
            Assert.That(time.IsTurnTracking, Is.False);

            time = Time(TimeOpKind.TimeEvent);
            Assert.That(time.LastEvent, Is.EqualTo("TimePasses"));
        }

        #endregion Time

        #region Describe and changes by hand

        [Test]
        public void Describe_AProperty()
        {
            var response = TestServer.Send<DescribeProp, RpgPropertyDescription>(_sheet,
                new DescribeProp { ObjectId = _view.ActorId, Prop = "CurrentStaminaPoints" },
                r => _server.Describe(System, r));

            Assert.That(response.Sheet, Is.EqualTo(_sheet), "describing does not change the sheet");
            Assert.That(response.Data.Value, Is.EqualTo(new Dice(14)));
            Assert.That(response.Data.ToText(), Does.Contain("Twice health (1 gives 2)"));

            var shallow = TestServer.Send<DescribeProp, RpgPropertyDescription>(_sheet,
                new DescribeProp { ObjectId = _view.ActorId, Prop = "CurrentStaminaPoints", Depth = 1 },
                r => _server.Describe(System, r));

            Assert.That(shallow.Data.Mods.Single().SourceProperty!.IsTruncated, Is.True);

            Assert.Throws<RpgServerException>(() => _server.Describe(System, new RpgRequest<DescribeProp>
            {
                Sheet = _sheet,
                Op = new DescribeProp { ObjectId = _view.ActorId, Prop = "Shoe size" }
            }));
        }

        [Test]
        public void Describe_AStateAndAnObject()
        {
            var state = TestServer.Send<DescribeState, RpgModSetDescription>(_sheet,
                new DescribeState { ObjectId = _view.ActorId, State = nameof(VeryFast) },
                r => _server.Describe(System, r));

            Assert.That(state.Data.IsOn, Is.False);
            Assert.That(state.Data.Changes.Single().Prop, Is.EqualTo("ActionPoints"));

            var obj = TestServer.Send<DescribeObject, RpgObjectDescription>(_sheet,
                new DescribeObject { ObjectId = Sword.Description.ObjectId },
                r => _server.Describe(System, r));

            Assert.That(obj.Data.Name, Is.EqualTo("Excalibur"));
        }

        [Test]
        public void ChangeByHand_AndUndo()
        {
            var changed = TestServer.Send<ChangeByHand, RpgManualChangeView>(_sheet,
                new ChangeByHand { ObjectId = _view.ActorId, Prop = "Strength", Value = "2", Kind = ChangeByHandKind.Override },
                r => _server.ChangeByHand(System, r));

            _sheet = changed.Sheet;
            Assert.That(changed.Data.Kind, Is.EqualTo("Override"));
            Assert.That(changed.Data.Value, Is.EqualTo(new Dice(2)));

            Refresh();
            Assert.That(Value(Benny, "Strength"), Is.EqualTo(new Dice(2)));
            Assert.That(Value(Benny, "LifePoints"), Is.EqualTo(new Dice(8)));
            Assert.That(_view.ManualChanges.Single().ModId, Is.EqualTo(changed.Data.ModId));
            Assert.That(Benny.Description.Properties.Single(x => x.Prop == "Strength").IsChanged, Is.True);

            var undone = TestServer.Send<UndoChangeByHand, bool>(_sheet,
                new UndoChangeByHand { ModId = changed.Data.ModId },
                r => _server.UndoChangeByHand(System, r));

            _sheet = undone.Sheet;
            Assert.That(undone.Data, Is.True);

            Refresh();
            Assert.That(Value(Benny, "Strength"), Is.EqualTo(new Dice(-1)));
            Assert.That(_view.ManualChanges, Is.Empty);

            Assert.Throws<RpgServerException>(() => _server.ChangeByHand(System, new RpgRequest<ChangeByHand>
            {
                Sheet = _sheet,
                Op = new ChangeByHand { ObjectId = _view.ActorId, Prop = "Strength", Value = "lots" }
            }));
        }

        [Test]
        public void SetState_ByHand()
        {
            SetStateMode? last = null;
            RpgModSetDescription Set(SetStateMode mode)
            {
                last = mode;
                var response = TestServer.Send<SetState, RpgModSetDescription>(_sheet,
                    new SetState { ObjectId = _view.ActorId, State = nameof(VeryFast), Mode = mode },
                    r => _server.SetState(System, r));

                _sheet = response.Sheet;
                return response.Data;
            }

            var on = Set(SetStateMode.On);
            Assert.That(on.IsOn, Is.True);
            Assert.That(on.Reason, Is.EqualTo(RpgStateReason.SwitchedOnByHand));

            Refresh();
            Assert.That(Value(Benny, "ActionPoints"), Is.EqualTo(new Dice(2)));

            var off = Set(SetStateMode.Off);
            Assert.That(off.Reason, Is.EqualTo(RpgStateReason.SwitchedOffByHand));

            var auto = Set(SetStateMode.Auto);
            Assert.That(auto.IsOn, Is.False);
            Assert.That(auto.Reason, Is.EqualTo(RpgStateReason.ConditionNotMet));
            Assert.That(last, Is.EqualTo(SetStateMode.Auto));
        }

        [Test]
        public void Settings_AreKeptOnTheSheet()
        {
            var response = TestServer.Send<SheetSettings, RpgSheetView>(_sheet,
                new SheetSettings { RollMode = RpgRollMode.App, MaxTurnHistory = 2 },
                r => _server.Settings(System, r));

            _sheet = response.Sheet;

            Assert.That(response.Data.RollMode, Is.EqualTo(RpgRollMode.App));
            Assert.That(response.Data.MaxTurnHistory, Is.EqualTo(2));

            Time(TimeOpKind.BeginTurnTracking);
            var time = Time(TimeOpKind.AdvanceToTurn, 5);

            Assert.That(time.RewindableTurns, Is.EqualTo(new[] { 4, 5 }));
            Assert.That(Refresh().RollMode, Is.EqualTo(RpgRollMode.App));
        }

        #endregion Describe and changes by hand

        #region Json

        [Test]
        public void Json_IsPlainData()
        {
            var json = RpgServerJson.Serialize(_server.GetSheet(System, new RpgRequest<object?> { Sheet = _sheet }));

            Assert.That(json, Does.Not.Contain("$type"));
            Assert.That(json, Does.Contain("\"rollMode\":\"Ask\""));
            Assert.That(json, Does.Contain("\"value\":\"1d6\""));
            Assert.That(json, Does.Contain("\"Hands\":["), "names of the game system's own properties are left as they are");
        }

        #endregion Json
    }
}
