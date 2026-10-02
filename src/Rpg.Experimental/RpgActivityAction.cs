using Newtonsoft.Json;
using Rpg.Experimental.Graph;
using Rpg.Experimental.Mods;
using Rpg.Experimental.Reflection.Args;

namespace Rpg.Experimental
{
    public sealed class RpgActivityAction : RpgObject
    {
        private RpgAction? _action;
        private RpgActivity? _activity;

        /// <summary>
        /// The costs of performing the action. Filled by the action's Cost step and applied when the action
        /// is completed. Costs measured in turns are skipped if turns are not being tracked.
        /// </summary>
        [JsonIgnore] public RpgModSet CostSet { get; private set; }

        /// <summary>
        /// The effects of the action. Filled by the action's Outcome step and applied when the action is completed.
        /// </summary>
        [JsonIgnore] public RpgModSet Result { get; private set; }

        [JsonProperty] public string ActionId { get; private set; }
        [JsonProperty] public string ActionOwnerId { get; private set; }
        [JsonProperty] public int ActivityActionNo { get; private set; }

        [JsonProperty] public RpgActionMethod CostMethod { get; private set; } = new RpgActionMethod();
        [JsonProperty] public RpgActionMethod PerformMethod { get; private set; } = new RpgActionMethod();
        [JsonProperty] public RpgActionMethod OutcomeMethod { get; private set; } = new RpgActionMethod();

        /// <summary>
        /// Actions nominated by the Outcome step to follow this one in the activity
        /// </summary>
        [JsonProperty] public List<RpgActionRef> OutcomeActions { get; private set; } = new();

        /// <summary>
        /// Costs that were not charged when the action was completed because they are measured in turns and
        /// turns were not being tracked. They are kept so they can be shown, and applied by hand with
        /// ApplySkippedCosts().
        /// </summary>
        [JsonProperty] public List<Mod> SkippedCosts { get; private set; } = new();

        /// <summary>
        /// Effects that were dropped when the action was completed because they last a number of turns, are
        /// too minor to start counting turns for, and turns were not being tracked. They are kept so they
        /// can be shown.
        /// </summary>
        [JsonProperty] public List<Mod> DroppedEffects { get; private set; } = new();

        [JsonProperty] public bool IsComplete { get; private set; }

        public bool CanAutoComplete { get => _action != null && !IsComplete && AllStepArgsComplete; }
        public bool AllStepsComplete { get => CostMethod.IsDone && PerformMethod.IsDone && OutcomeMethod.IsDone; }
        public bool AllStepArgsComplete { get => CostMethod.Args.IsComplete() && PerformMethod.Args.IsComplete() && OutcomeMethod.Args.IsComplete(); }

        [JsonConstructor] private RpgActivityAction() { }

        public RpgActivityAction(RpgActivity owner, RpgAction action, int activityActionNo)
            : base(owner.Id, true)
        {
            _action = action;
            _activity = owner;

            ActionId = action.Id;
            ActionOwnerId = action.OwnerId!;
            ActivityActionNo = activityActionNo;
        }

        public bool Cost(RpgGraph graph, params (string, object?)[] args)
        {
            SetArgValues(graph, args, CostMethod, PerformMethod, OutcomeMethod);
            RollForStep(graph, CostMethod, ActionMethodNames.Cost);

            if (!CostMethod.Args.IsComplete())
                return false;

            if (CostMethod.IsDone)
                return true;

            var res = CostMethod.Execute(graph);
            graph.Time.Refresh();

            return res;
        }

        public bool Perform(RpgGraph graph, params (string, object?)[] args)
        {
            SetArgValues(graph, args, PerformMethod, OutcomeMethod);
            RollForStep(graph, PerformMethod, ActionMethodNames.Perform);

            if (!PerformMethod.Args.IsComplete())
                return false;

            //Already performed, or the action has no Perform step
            if (PerformMethod.IsDone)
                return true;

            var res = PerformMethod.Execute(graph);
            graph.Time.Refresh();

            return res;
        }

        public bool Outcome(RpgGraph graph, params (string, object?)[] args)
        {
            SetArgValues(graph, args, OutcomeMethod);
            RollForStep(graph, OutcomeMethod, ActionMethodNames.Outcome);

            if (!OutcomeMethod.Args.IsComplete())
                return false;

            if (OutcomeMethod.IsDone)
                return true;

            var res = OutcomeMethod.Execute(graph);
            graph.Time.Refresh();

            return res;
        }

        /// <summary>
        /// Make a step (and the steps after it) ready to be done again. Anything the steps added to the
        /// cost and result sets is removed.
        /// </summary>
        public void Reset(RpgGraph graph, string methodName)
        {
            OutcomeMethod.Reset(this);
            OutcomeActions.Clear();
            Result.Clear(graph);

            if (methodName == ActionMethodNames.Perform)
                PerformMethod.Reset(this);

            if (methodName == ActionMethodNames.Cost)
            {
                CostMethod.Reset(this);
                PerformMethod.Reset(this);

                CostSet.Clear(graph);
                SkippedCosts.Clear();
            }

            DroppedEffects.Clear();
            graph.Time.Refresh();
        }

        public RpgAction? GetAction()
            => _action;

        /// <summary>
        /// The rolls the action still needs before its steps can run. A step with a pending roll does not
        /// run: settle the roll with RpgGraph.Roll() or RpgGraph.SetRoll(), or supply the result of the
        /// dice with the step.
        /// </summary>
        public RpgPendingRoll[] GetPendingRolls(RpgGraph graph, string? step = null)
            => graph.GetPendingRolls(Id)
                .Where(x => step == null || x.Steps.Contains(step))
                .ToArray();

        /// <summary>
        /// The app rolls what the action (or one of its steps) still needs
        /// </summary>
        public RpgRoll[] RollPending(RpgGraph graph, string? step = null)
        {
            var rolls = GetPendingRolls(graph, step)
                .Select(x => graph.GetPropertyData<RpgPropertyDataModdable>(Id, x.Prop)?.SetRoll(graph, RpgRollSource.App))
                .Where(x => x != null)
                .Cast<RpgRoll>()
                .ToArray();

            if (rolls.Any())
                SetArgValues(graph, null, CostMethod, PerformMethod, OutcomeMethod);

            return rolls;
        }

        /// <summary>
        /// The steps still to be done that take the property as a whole number
        /// </summary>
        internal string[] StepsNeedingNumber(string prop)
        {
            var steps = new List<string>();
            if (NeedsNumber(CostMethod, prop)) steps.Add(ActionMethodNames.Cost);
            if (NeedsNumber(PerformMethod, prop)) steps.Add(ActionMethodNames.Perform);
            if (NeedsNumber(OutcomeMethod, prop)) steps.Add(ActionMethodNames.Outcome);

            return steps.ToArray();
        }

        private static bool NeedsNumber(RpgActionMethod method, string prop)
            => !method.IsDone && method.Args.Any(x => x.Name == prop && x is IntegerArg);

        /// <summary>
        /// If the sheet is set to let the app roll, running a step rolls what the step still needs
        /// </summary>
        private void RollForStep(RpgGraph graph, RpgActionMethod method, string step, bool always = false)
        {
            if (method.IsDone || (!always && graph.RollMode != RpgRollMode.App))
                return;

            RollPending(graph, step);
        }

        /// <summary>
        /// True if every step has what it needs apart from rolls that are still pending
        /// </summary>
        public bool CanAutoCompleteWithRolls(RpgGraph graph)
        {
            if (_action == null || IsComplete)
                return false;

            var pending = GetPendingRolls(graph).Select(x => x.Prop).ToArray();
            foreach (var method in new[] { CostMethod, PerformMethod, OutcomeMethod })
                foreach (var arg in method.Args)
                    if (!RpgArg.IsGraphArg(arg.Name) && !arg.IsNullable && arg.Value == null && !pending.Contains(arg.Name))
                        return false;

            return true;
        }

        /// <summary>
        /// Called from an action's Outcome method to nominate an action that should follow this one.
        /// The nominated actions are returned by Complete() and can be started with RpgGraph.CreateActivity().
        /// </summary>
        public RpgActivityAction SetOutcomeAction(RpgObject actionOwner, string actionName, bool optional)
        {
            OutcomeActions = OutcomeActions
                .Where(x => x.ActionOwnerId != actionOwner.Id || x.ActionName != actionName)
                .ToList();

            OutcomeActions.Add(new RpgActionRef(actionOwner.Id, actionName, optional));

            return this;
        }

        public override void OnCreating(RpgGraph graph, RpgObject? obj)
        {
            base.OnCreating(graph, obj);

            CostMethod.OnCreating(graph, _action, _action?.CostMethod);
            PerformMethod.OnCreating(graph, _action, _action?.PerformMethod);
            OutcomeMethod.OnCreating(graph, _action, _action?.OutcomeMethod);

            //Int and Dice args become virtual properties of this activity action so that their values are
            //built from mods like any other property
            graph.CreateVirtualProperties(this, CostMethod.Args);
            graph.CreateVirtualProperties(this, PerformMethod.Args);
            graph.CreateVirtualProperties(this, OutcomeMethod.Args);
            _action?.OnCreatingActivityAction(graph, this);

            RestoreModSets(graph);
        }

        public override void OnRestoring(RpgGraph graph)
        {
            base.OnRestoring(graph);

            _action = graph.GetObject(ActionId) as RpgAction;
            _activity = graph.GetObject(OwnerId) as RpgActivity;

            CostMethod.OnRestoring(graph, _action, _action?.CostMethod);
            PerformMethod.OnRestoring(graph, _action, _action?.PerformMethod);
            OutcomeMethod.OnRestoring(graph, _action, _action?.OutcomeMethod);

            RestoreModSets(graph);
        }

        public override void OnTimeEvent(RpgGraph graph)
        {
            base.OnTimeEvent(graph);

            RpgArg.SetValues(graph, CostMethod.Args, this);
            RpgArg.SetValues(graph, PerformMethod.Args, this);
            RpgArg.SetValues(graph, OutcomeMethod.Args, this);
        }

        public override RpgObject? ResolvePropertyNameToObject(RpgGraph graph, string prop)
        {
            RpgObject? obj = prop switch
            {
                ActionReservedArgs.Context => graph.Context,
                ActionReservedArgs.Owner => graph.GetObject(ActionOwnerId),
                ActionReservedArgs.Actor => (graph as RpgCharacterSheet)?.Actor,
                ActionReservedArgs.Action => graph.GetObject(ActionId),
                ActionReservedArgs.Activity => graph.GetObject(OwnerId),
                ActionReservedArgs.ActivityAction => this,
                _ => null
            };

            return obj;
        }

        private void RestoreModSets(RpgGraph graph)
        {
            CostSet ??= RestoreModSet(graph, ActionMethodNames.Cost);
            Result ??= RestoreModSet(graph, ActionMethodNames.Outcome);
        }

        private RpgModSet RestoreModSet(RpgGraph graph, string name)
        {
            var modSet = graph.GetOwnerModSets(Id)?.FirstOrDefault(x => x.Name == name);
            if (modSet == null)
            {
                modSet = new RpgModSet(name, Id, false)
                    .Lifespan(Start, End);

                modSet.Unapply();
                graph.Add(modSet);
            }

            return modSet;
        }

        /// <summary>
        /// Apply the results and the costs of the action. Returns the actions nominated by the outcome to
        /// follow this one.
        ///
        /// The results are applied first. A result that lasts a number of turns starts turn tracking if it
        /// is not already on. A cost measured in turns is then skipped only if turns are still not being
        /// tracked.
        /// </summary>
        public RpgActionRef[] Complete(RpgGraph graph)
        {
            if (IsComplete)
                return OutcomeActions.ToArray();

            if (!AllStepsComplete)
                return [];

            var wasTurnTracking = graph.Time.IsTurnTracking;
            var minorEffects = Result.Mods
                .Where(x => x.IsTurnBased && !x.StartsTurnTracking)
                .ToArray();

            Result.Apply();
            IsComplete = true;
            graph.Time.Refresh();

            //Still no turn tracking, so the minor effects were dropped rather than left to run
            if (!graph.Time.IsTurnTracking)
                DroppedEffects.AddRange(minorEffects.Where(x => !DroppedEffects.Any(d => d.Id == x.Id)));

            if (!graph.Time.IsTurnTracking)
            {
                foreach (var cost in CostSet.Mods.Where(x => x.IsTurnBased).ToArray())
                {
                    CostSet.Remove(graph, cost);
                    SkippedCosts.Add(cost);
                }
            }

            CostSet.Apply();
            graph.Time.Refresh();

            //The action started turn tracking, so the start of the first turn includes the whole action
            if (!wasTurnTracking && graph.Time.IsTurnTracking)
                graph.OnTurnStateChanged();

            return OutcomeActions.ToArray();
        }

        /// <summary>
        /// Charge the costs that were skipped when the action was completed
        /// </summary>
        public void ApplySkippedCosts(RpgGraph graph)
        {
            if (!SkippedCosts.Any())
                return;

            foreach (var cost in SkippedCosts)
                CostSet.Add(cost);

            SkippedCosts.Clear();
            graph.Time.Refresh();
        }

        public RpgActionRef[] AutoComplete(RpgGraph graph, params (string, object?)[]? args)
        {
            SetArgValues(graph, args, CostMethod, PerformMethod, OutcomeMethod);

            //Auto completing lets the app roll whatever is still pending. Each step's rolls are made just
            //before the step, and stay on the action so they can be seen afterwards.
            if (CanAutoCompleteWithRolls(graph))
            {
                RollForStep(graph, CostMethod, ActionMethodNames.Cost, true);
                if (CostMethod.Execute(graph))
                    graph.Time.Refresh();

                RollForStep(graph, PerformMethod, ActionMethodNames.Perform, true);
                if (PerformMethod.Execute(graph))
                    graph.Time.Refresh();

                RollForStep(graph, OutcomeMethod, ActionMethodNames.Outcome, true);
                if (OutcomeMethod.Execute(graph))
                    graph.Time.Refresh();
            }

            return Complete(graph);
        }

        /// <summary>
        /// Undo the whole action: every step can be done again and nothing it added remains
        /// </summary>
        public void Reset(RpgGraph graph)
        {
            CostMethod.Reset(this);
            PerformMethod.Reset(this);
            OutcomeMethod.Reset(this);

            Result.Clear(graph);
            Result.Unapply();

            CostSet.Clear(graph);
            CostSet.Unapply();

            SkippedCosts.Clear();
            DroppedEffects.Clear();
            OutcomeActions.Clear();
            IsComplete = false;

            foreach (var propData in graph.GetPropertyData<RpgPropertyDataModdable>(Id))
                propData.ClearRoll(graph);

            graph.Time.Refresh();
        }

        /// <summary>
        /// Int and Dice arg values are stored as mods on this activity action's virtual properties. Any other
        /// supplied values (e.g. objects) are set directly on the method args. The method args are then refreshed
        /// from the virtual properties and reserved names (owner, actor, etc).
        /// </summary>
        private void SetArgValues(RpgGraph graph, (string, object?)[]? args, params RpgActionMethod[] methods)
        {
            if (args != null && args.Length > 0)
            {
                graph.SyncVirtualPropertyValues(this, args);

                var directArgs = args
                    .Where(x => graph.GetPropertyData<RpgPropertyDataModdable>(Id, x.Item1) == null)
                    .ToArray();

                foreach (var method in methods)
                    RpgArg.SetValues(graph, method.Args, directArgs);
            }

            foreach (var method in methods)
                RpgArg.SetValues(graph, method.Args, this);
        }
    }
}
