using Newtonsoft.Json.Linq;
using Rpg.Experimental.Description;
using Rpg.Experimental.Graph;
using Rpg.Experimental.System;
using Clock = Rpg.Experimental.Time.Temporal;

namespace Rpg.Experimental.Server
{
    /// <summary>
    /// The operations a character sheet user interface performs on a character sheet.
    ///
    /// It keeps nothing between calls: each request carries the whole sheet as text, and each response
    /// carries the sheet back as it is afterwards. It has no dependency on a web framework, so the same
    /// class can sit behind a web api or run on the device itself.
    /// </summary>
    public class RpgSessionlessServer
    {
        private readonly RpgSystems _systems;
        private readonly IContentFactory? _contentFactory;

        public RpgSessionlessServer(RpgSystems systems, IContentFactory? contentFactory = null)
        {
            _systems = systems;
            _contentFactory = contentFactory;
        }

        #region Systems and content

        public RpgSystem[] ListSystems()
            => _systems.All();

        public RpgSystem GetSystem(string system)
            => _systems.Get(system);

        public RpgContent[] ListEntities(string system)
        {
            _systems.Get(system);
            return ContentFactory().ListEntities(system);
        }

        /// <summary>
        /// A new character sheet for something authored, e.g. a character from the content library
        /// </summary>
        public RpgResponse<RpgSheetView> CreateSheet(string system, string archetype, string contentId)
        {
            var rpgSystem = _systems.Get(system);
            var actor = ContentFactory().CreateEntity(system, archetype, contentId);

            return CreateSheet(rpgSystem, actor);
        }

        /// <summary>
        /// A new character sheet for an object created by the caller
        /// </summary>
        public RpgResponse<RpgSheetView> CreateSheet(string system, RpgObject actor)
            => CreateSheet(_systems.Get(system), actor);

        private RpgResponse<RpgSheetView> CreateSheet(RpgSystem rpgSystem, RpgObject actor)
        {
            var sheet = new RpgCharacterSheet(actor, rpgSystem);
            return Respond(sheet, SheetView(sheet));
        }

        public RpgResponse<RpgSheetView> GetSheet(string system, RpgRequest<object?> request)
        {
            var sheet = Load(system, request.Sheet);
            return Respond(sheet, SheetView(sheet));
        }

        public RpgResponse<RpgSheetView> Settings(string system, RpgRequest<SheetSettings> request)
        {
            var sheet = Load(system, request.Sheet);

            if (request.Op.RollMode != null)
                sheet.RollMode = request.Op.RollMode.Value;

            if (request.Op.MaxTurnHistory != null)
                sheet.MaxTurnHistory = request.Op.MaxTurnHistory.Value;

            return Respond(sheet, SheetView(sheet));
        }

        #endregion Systems and content

        #region Describe

        public RpgResponse<RpgPropertyDescription> Describe(string system, RpgRequest<DescribeProp> request)
        {
            var sheet = Load(system, request.Sheet);
            var obj = GetObject(sheet, request.Op.ObjectId);

            var description = sheet.Describe(obj, request.Op.Prop, request.Op.Depth ?? int.MaxValue)
                ?? throw new RpgServerException($"{request.Op.ObjectId} has no property '{request.Op.Prop}' that can be described");

            return Respond(request.Sheet, description);
        }

        public RpgResponse<RpgModSetDescription> Describe(string system, RpgRequest<DescribeState> request)
        {
            var sheet = Load(system, request.Sheet);
            var obj = GetObject(sheet, request.Op.ObjectId);

            var description = sheet.DescribeState(obj, request.Op.State)
                ?? throw new RpgServerException($"{request.Op.ObjectId} has no state '{request.Op.State}'");

            return Respond(request.Sheet, description);
        }

        public RpgResponse<RpgModSetDescription> Describe(string system, RpgRequest<DescribeModSet> request)
        {
            var sheet = Load(system, request.Sheet);
            var modSet = sheet.GetLifecycleObject(request.Op.ModSetId) as RpgModSet
                ?? throw new RpgServerException($"Mod set {request.Op.ModSetId} not found");

            return Respond(request.Sheet, sheet.DescribeModSet(modSet));
        }

        public RpgResponse<RpgObjectDescription> Describe(string system, RpgRequest<DescribeObject> request)
        {
            var sheet = Load(system, request.Sheet);
            var obj = GetObject(sheet, request.Op.ObjectId);

            return Respond(request.Sheet, sheet.DescribeObject(obj));
        }

        public RpgResponse<RpgActionDescription> Describe(string system, RpgRequest<DescribeAction> request)
        {
            var sheet = Load(system, request.Sheet);
            var activityAction = GetActivityAction(sheet, request.Op.ActivityActionId);

            return Respond(request.Sheet, sheet.DescribeAction(activityAction));
        }

        #endregion Describe

        #region Changes by hand

        public RpgResponse<RpgManualChangeView> ChangeByHand(string system, RpgRequest<ChangeByHand> request)
        {
            var sheet = Load(system, request.Sheet);
            var obj = GetObject(sheet, request.Op.ObjectId);

            if (!TryParseDice(request.Op.Value, out var value))
                throw new RpgServerException($"'{request.Op.Value}' is not a number or a dice expression");

            if (sheet.GetPropertyData<RpgPropertyDataModdable>(obj.Id, request.Op.Prop) == null)
                throw new RpgServerException($"{request.Op.ObjectId} has no property '{request.Op.Prop}' that can be changed");

            var mod = request.Op.Kind == ChangeByHandKind.Override
                ? sheet.OverrideByHand(obj, request.Op.Prop, value)
                : sheet.AdjustByHand(obj, request.Op.Prop, value);

            return Respond(sheet, ManualChangeView(mod));
        }

        public RpgResponse<bool> UndoChangeByHand(string system, RpgRequest<UndoChangeByHand> request)
        {
            var sheet = Load(system, request.Sheet);

            var mod = sheet.GetManualChanges().FirstOrDefault(x => x.Id == request.Op.ModId);
            if (mod != null)
                sheet.RemoveManualChange(mod);

            return Respond(sheet, mod != null);
        }

        public RpgResponse<RpgModSetDescription> SetState(string system, RpgRequest<SetState> request)
        {
            var sheet = Load(system, request.Sheet);
            var obj = GetObject(sheet, request.Op.ObjectId);

            var state = sheet.GetObjectState(obj.Id, request.Op.State)
                ?? throw new RpgServerException($"{request.Op.ObjectId} has no state '{request.Op.State}'");

            switch (request.Op.Mode)
            {
                case SetStateMode.On: state.UserEnabled(); break;
                case SetStateMode.Off: state.UserDisabled(); break;
                default: state.UserEnabledReset(); break;
            }

            sheet.Time.Refresh();

            return Respond(sheet, sheet.DescribeModSet(state));
        }

        #endregion Changes by hand

        #region Time

        public RpgResponse<RpgTimeView> Time(string system, RpgRequest<TimeOp> request)
        {
            var sheet = Load(system, request.Sheet);
            var op = request.Op;

            switch (op.Kind)
            {
                case TimeOpKind.BeginTurnTracking:
                    sheet.BeginTurnTracking(op.Turn ?? 1);
                    break;

                case TimeOpKind.NextTurn:
                    sheet.NextTurn();
                    break;

                case TimeOpKind.AdvanceToTurn:
                    sheet.AdvanceToTurn(RequireTurn(op));
                    break;

                case TimeOpKind.RenumberTurn:
                    sheet.RenumberTurn(RequireTurn(op));
                    break;

                case TimeOpKind.EndTurnTracking:
                    sheet.EndTurnTracking();
                    break;

                case TimeOpKind.TimeEvent:
                    sheet.TriggerTimeEvent(string.IsNullOrEmpty(op.Event) ? Clock.TimePassesEvent : op.Event);
                    break;

                case TimeOpKind.RewindToTurn:
                    if (!sheet.RewindToTurn(RequireTurn(op)))
                        throw new RpgServerException($"Turn {op.Turn} cannot be gone back to. The turns that can are: {string.Join(", ", sheet.GetRewindableTurns())}");
                    break;
            }

            return Respond(sheet, TimeView(sheet));
        }

        private static int RequireTurn(TimeOp op)
            => op.Turn ?? throw new RpgServerException($"{op.Kind} needs a turn number");

        #endregion Time

        #region Actions

        public RpgResponse<RpgActionResult> InitiateAction(string system, RpgRequest<InitiateAction> request)
        {
            var sheet = Load(system, request.Sheet);
            var initiator = GetObject(sheet, request.Op.InitiatorId);
            var actionOwner = GetObject(sheet, request.Op.ActionOwnerId);

            if (sheet.GetObjectAction(actionOwner.Id, request.Op.ActionName) == null)
                throw new RpgServerException($"{request.Op.ActionOwnerId} has no action '{request.Op.ActionName}'");

            var activity = sheet.CreateActivity(initiator.Id, actionOwner.Id, request.Op.ActionName);
            var activityAction = activity.CurrentActivityAction!;

            return Respond(sheet, ActionResult(sheet, activityAction, true));
        }

        public RpgResponse<RpgActionResult> ActionStep(string system, RpgRequest<ActionStepRun> request)
        {
            var sheet = Load(system, request.Sheet);
            var activityAction = GetActivityAction(sheet, request.Op.ActivityActionId);
            var args = ToArgs(request.Op.Args);

            var ran = request.Op.Step switch
            {
                ActionMethodNames.Cost => activityAction.Cost(sheet, args),
                ActionMethodNames.Perform => activityAction.Perform(sheet, args),
                ActionMethodNames.Outcome => activityAction.Outcome(sheet, args),
                _ => throw new RpgServerException($"'{request.Op.Step}' is not a step. Use Cost, Perform or Outcome")
            };

            return Respond(sheet, ActionResult(sheet, activityAction, ran));
        }

        public RpgResponse<RpgActionResult> ActionComplete(string system, RpgRequest<ActionComplete> request)
        {
            var sheet = Load(system, request.Sheet);
            var activityAction = GetActivityAction(sheet, request.Op.ActivityActionId);

            activityAction.Complete(sheet);

            return Respond(sheet, ActionResult(sheet, activityAction, activityAction.IsComplete));
        }

        public RpgResponse<RpgActionResult> ActionAutoComplete(string system, RpgRequest<ActionAutoComplete> request)
        {
            var sheet = Load(system, request.Sheet);
            var activityAction = GetActivityAction(sheet, request.Op.ActivityActionId);

            activityAction.AutoComplete(sheet, ToArgs(request.Op.Args));

            return Respond(sheet, ActionResult(sheet, activityAction, activityAction.IsComplete));
        }

        public RpgResponse<RpgActionResult> ActionReset(string system, RpgRequest<ActionReset> request)
        {
            var sheet = Load(system, request.Sheet);
            var activityAction = GetActivityAction(sheet, request.Op.ActivityActionId);

            if (string.IsNullOrEmpty(request.Op.Step))
                activityAction.Reset(sheet);
            else
                activityAction.Reset(sheet, request.Op.Step);

            return Respond(sheet, ActionResult(sheet, activityAction, true));
        }

        public RpgResponse<RpgActionResult> ApplySkippedCosts(string system, RpgRequest<ActionComplete> request)
        {
            var sheet = Load(system, request.Sheet);
            var activityAction = GetActivityAction(sheet, request.Op.ActivityActionId);

            activityAction.ApplySkippedCosts(sheet);

            return Respond(sheet, ActionResult(sheet, activityAction, true));
        }

        #endregion Actions

        #region Rolls

        public RpgResponse<RpgPendingRoll[]> Roll(string system, RpgRequest<RollOp> request)
        {
            var sheet = Load(system, request.Sheet);
            var op = request.Op;

            if (op.Kind == RollOpKind.RollAllPending)
            {
                sheet.RollPending();
            }
            else
            {
                var obj = GetObject(sheet, op.ObjectId);
                var prop = op.Prop ?? throw new RpgServerException($"{op.Kind} needs a property");

                switch (op.Kind)
                {
                    case RollOpKind.Roll:
                        sheet.Roll(obj.Id, prop);
                        break;

                    case RollOpKind.Set:
                        sheet.SetRoll(obj.Id, prop, op.Result ?? throw new RpgServerException("Set needs the result of the dice"));
                        break;

                    case RollOpKind.Clear:
                        sheet.ClearRoll(obj.Id, prop);
                        break;
                }
            }

            return Respond(sheet, sheet.GetPendingRolls());
        }

        #endregion Rolls

        #region Views

        private RpgSheetView SheetView(RpgCharacterSheet sheet)
        {
            var view = new RpgSheetView
            {
                System = sheet.GetSystem().Identifier,
                ActorId = sheet.Actor.Id,
                Time = TimeView(sheet),
                RollMode = sheet.RollMode,
                MaxTurnHistory = sheet.MaxTurnHistory,
                PendingRolls = sheet.GetPendingRolls().ToList(),
                ManualChanges = sheet.GetManualChanges().Select(ManualChangeView).ToList()
            };

            //The things on the sheet: the character, its parts and its gear. Not the engine's own machinery.
            var objects = sheet.GetObjects<RpgObject>()
                .Where(x => x is not RpgAction && x is not RpgActivity && x is not RpgActivityAction)
                .Where(x => x.Expiry == LifecycleExpiry.Active);

            foreach (var obj in objects)
            {
                var objectView = new RpgObjectView
                {
                    Description = sheet.DescribeObject(obj),
                    Actions = sheet.GetObjectActions(obj.Id)
                        .Select(x => new RpgActionView { Name = x.Name, OwnerId = obj.Id, IsPerformable = x.IsPerformable })
                        .OrderBy(x => x.Name)
                        .ToList()
                };

                foreach (var childProp in sheet.GetPropertyData<RpgPropertyDataObject>(obj.Id))
                    objectView.Children[childProp.Prop] = childProp.GetProperty(sheet).ChildObjectIds ?? [];

                view.Objects.Add(objectView);
            }

            view.ActionsInProgress = sheet.GetObjects<RpgActivityAction>()
                .Where(x => !x.IsComplete && x.Expiry == LifecycleExpiry.Active)
                .Select(x => sheet.DescribeAction(x, 1))
                .ToList();

            return view;
        }

        private static RpgTimeView TimeView(RpgCharacterSheet sheet)
        {
            var report = sheet.GetTurnTrackingReport();

            return new RpgTimeView
            {
                IsTurnTracking = sheet.Time.IsTurnTracking,
                Turn = sheet.Time.Turn,
                LastEvent = sheet.Time.LastEvent,
                RewindableTurns = sheet.GetRewindableTurns(),
                TimeEvents = [.. sheet.GetSystem().TimeEvents, Clock.TimePassesEvent],
                TurnTrackingNeeded = report.IsNeeded,
                TurnTrackingNeededFor = report.States
                    .Select(x => $"State {x.Name}")
                    .Concat(report.Effects.Select(x => $"{x.Name ?? "An effect"} on {x.Target}"))
                    .ToArray()
            };
        }

        private static RpgManualChangeView ManualChangeView(Mods.Mod mod)
            => new RpgManualChangeView
            {
                ModId = mod.Id,
                ObjectId = mod.Target?.ObjectId ?? string.Empty,
                Prop = mod.Target?.Path ?? string.Empty,
                Kind = mod.Type == Mods.ModType.Override ? nameof(ChangeByHandKind.Override) : nameof(ChangeByHandKind.Adjust),
                Value = mod.Source?.Value
            };

        private static RpgActionResult ActionResult(RpgCharacterSheet sheet, RpgActivityAction activityAction, bool ran)
            => new RpgActionResult
            {
                Ran = ran,
                Action = sheet.DescribeAction(activityAction),
                FollowOnActions = activityAction.IsComplete ? activityAction.OutcomeActions.ToArray() : []
            };

        #endregion Views

        #region Helpers

        private IContentFactory ContentFactory()
            => _contentFactory ?? throw new RpgServerException("This host has no content library");

        private RpgCharacterSheet Load(string system, string? saved)
        {
            var rpgSystem = _systems.Get(system);

            if (string.IsNullOrWhiteSpace(saved))
                throw new RpgServerException("The request has no character sheet");

            try
            {
                return RpgCharacterSheet.Load(saved, rpgSystem);
            }
            catch (Exception ex) when (ex is FormatException || ex is InvalidDataException || ex is Newtonsoft.Json.JsonException)
            {
                throw new RpgServerException($"The character sheet could not be read: {ex.Message}");
            }
        }

        /// <summary>
        /// The dice parser treats text it does not understand as nothing. A value typed by a person is
        /// checked first so a slip is reported, not silently taken as zero.
        /// </summary>
        private static bool TryParseDice(string? text, out Dice dice)
        {
            dice = Dice.Zero;
            const string term = @"([0-9]*d([0-9]+|%)|[0-9]+)";
            var pattern = $@"^\s*[+-]?\s*{term}(\s*[+-]\s*{term})*\s*$";

            if (string.IsNullOrWhiteSpace(text) || !global::System.Text.RegularExpressions.Regex.IsMatch(text, pattern, global::System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                return false;

            return Dice.TryParse(text, out dice);
        }

        private static RpgResponse<T> Respond<T>(RpgCharacterSheet sheet, T data)
            => new RpgResponse<T> { Sheet = sheet.Save(), Data = data };

        /// <summary>
        /// For operations that only read: the sheet goes back exactly as it came
        /// </summary>
        private static RpgResponse<T> Respond<T>(string sheet, T data)
            => new RpgResponse<T> { Sheet = sheet, Data = data };

        private static RpgObject GetObject(RpgCharacterSheet sheet, string? objectId)
            => sheet.GetObject(objectId)
                ?? throw new RpgServerException($"Object {objectId} not found on the character sheet");

        private static RpgActivityAction GetActivityAction(RpgCharacterSheet sheet, string? activityActionId)
            => sheet.GetObject(activityActionId) as RpgActivityAction
                ?? throw new RpgServerException($"Action {activityActionId} not found on the character sheet");

        /// <summary>
        /// Values that have come through json arrive as longs, doubles and json tokens. The engine wants
        /// whole numbers and text.
        /// </summary>
        private static (string, object?)[] ToArgs(Dictionary<string, object?>? args)
        {
            if (args == null)
                return [];

            return args
                .Select(x => (x.Key, Normalise(x.Value)))
                .ToArray();
        }

        private static object? Normalise(object? value)
            => value switch
            {
                JValue jValue => Normalise(jValue.Value),
                JToken jToken => jToken.ToString(),
                long l => (int)l,
                double d when Math.Abs(d - Math.Round(d)) < 0.0000001 => (int)Math.Round(d),
                decimal m when m == Math.Round(m) => (int)m,
                global::System.Text.Json.JsonElement element => element.ValueKind switch
                {
                    global::System.Text.Json.JsonValueKind.Number => element.TryGetInt32(out var i) ? i : element.GetDouble(),
                    global::System.Text.Json.JsonValueKind.String => element.GetString(),
                    global::System.Text.Json.JsonValueKind.True => true,
                    global::System.Text.Json.JsonValueKind.False => false,
                    global::System.Text.Json.JsonValueKind.Null => null,
                    _ => element.ToString()
                },
                _ => value
            };

        #endregion Helpers
    }
}
