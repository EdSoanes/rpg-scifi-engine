using Newtonsoft.Json;
using Rpg.Experimental.Graph;
using Rpg.Experimental.Reflection.Args;

namespace Rpg.Experimental
{
    public sealed class RpgActivityAction : RpgObject
    {
        private RpgAction? _action;
        private RpgActivity? _activity;

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

            if (!PerformMethod.Args.IsComplete())
                return false;

            var res = PerformMethod.Execute(graph);
            graph.Time.Refresh();

            return res;
        }

        public bool Outcome(RpgGraph graph, params (string, object?)[] args)
        {
            SetArgValues(graph, args, OutcomeMethod);

            if (!OutcomeMethod.Args.IsComplete())
                return false;

            var res = OutcomeMethod.Execute(graph);
            graph.Time.Refresh();

            return res;
        }

        public void Reset(string methodName)
        {
            OutcomeMethod.Reset(this);
            OutcomeActions.Clear();

            if (methodName == ActionMethodNames.Perform)
                PerformMethod.Reset(this);

            if (methodName == ActionMethodNames.Cost)
            {
                CostMethod.Reset(this);
                PerformMethod.Reset(this);
            }
        }

        public RpgAction? GetAction()
            => _action;

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

            RestoreOutcome(graph);
        }

        public override void OnRestoring(RpgGraph graph)
        {
            base.OnRestoring(graph);

            _action = graph.GetObject(ActionId) as RpgAction;
            _activity = graph.GetObject(OwnerId) as RpgActivity;

            CostMethod.OnRestoring(graph, _action, _action?.CostMethod);
            PerformMethod.OnRestoring(graph, _action, _action?.PerformMethod);
            OutcomeMethod.OnRestoring(graph, _action, _action?.OutcomeMethod);

            RestoreOutcome(graph);
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

        private void RestoreOutcome(RpgGraph graph)
        {
            if (Result == null)
            {
                var resultSet = graph.GetOwnerModSets(Id)?.FirstOrDefault(x => x.Name == ActionMethodNames.Outcome);
                if (resultSet == null)
                {
                    resultSet = new RpgModSet(ActionMethodNames.Outcome, Id, false)
                        .Lifespan(Start, End);

                    resultSet.Unapply();
                    graph.Add(resultSet);
                }

                Result = resultSet;
            }
        }

        /// <summary>
        /// Apply the results of the action. Returns the actions nominated by the outcome to follow this one.
        /// </summary>
        public RpgActionRef[] Complete()
        {
            if (AllStepsComplete)
            {
                Result.Apply();
                IsComplete = true;

                return OutcomeActions.ToArray();
            }

            return [];
        }

        public RpgActionRef[] AutoComplete(RpgGraph graph, params (string, object?)[]? args)
        {
            SetArgValues(graph, args, CostMethod, PerformMethod, OutcomeMethod);

            if (CanAutoComplete)
            {
                if (CostMethod.Execute(graph))
                    graph.Time.Refresh();

                if (PerformMethod.Execute(graph))
                    graph.Time.Refresh();

                if (OutcomeMethod.Execute(graph))
                    graph.Time.Refresh();
            }

            return Complete();
        }

        public void Reset()
        {
            CostMethod.Reset(this);
            PerformMethod.Reset(this);
            OutcomeMethod.Reset(this);

            OutcomeActions.Clear();
            IsComplete = false;
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
