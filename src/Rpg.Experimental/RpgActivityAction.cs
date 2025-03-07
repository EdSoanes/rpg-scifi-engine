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

        [JsonProperty] public List<string> RecommendedActions { get; private set; } = new();

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
            graph.SyncVirtualPropertyValues(this, args);
            RpgArg.SetValues(graph, CostMethod.Args, this);
            RpgArg.SetValues(graph, PerformMethod.Args, this);
            RpgArg.SetValues(graph, OutcomeMethod.Args, this);

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
            graph.SyncVirtualPropertyValues(this, args);
            RpgArg.SetValues(graph, PerformMethod.Args, this);
            RpgArg.SetValues(graph, OutcomeMethod.Args, this);

            if (!PerformMethod.Args.IsComplete())
                return false;

            var res = PerformMethod.Execute(graph);
            graph.Time.Refresh();

            return res;
        }

        public bool Outcome(RpgGraph graph, params (string, object?)[] args)
        {
            graph.SyncVirtualPropertyValues(this, args);
            RpgArg.SetValues(graph, OutcomeMethod.Args, this);

            if (!OutcomeMethod.Args.IsComplete())
                return false;

            var res = OutcomeMethod.Execute(graph);
            graph.Time.Refresh();

            return res;
        }

        public void Reset(string methodName)
        {
            OutcomeMethod.Reset(this);
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

        public override void OnCreating(RpgGraph graph, RpgObject? obj)
        {
            base.OnCreating(graph, obj);

            graph.CreateVirtualProperties(this, CostMethod.Args);
            graph.CreateVirtualProperties(this, PerformMethod.Args);
            graph.CreateVirtualProperties(this, OutcomeMethod.Args);
            _action!.OnCreatingActivityAction(graph, this);

            RestoreOutcome(graph);
        }

        public override void OnRestoring(RpgGraph graph)
        {
            base.OnRestoring(graph);

            _action = graph.GetObject(ActionId) as RpgAction;
            _activity = graph.GetObject(OwnerId) as RpgActivity;

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

        public void Complete()
        {
            if (AllStepsComplete)
            {
                Result.Apply();
                IsComplete = true;
            }
        }

        public void AutoComplete(RpgGraph graph, params (string, object?)[]? args)
        {
            RpgArg.SetValue(graph, CostMethod.Args, args);
            RpgArg.SetValue(graph, PerformMethod.Args, args);
            RpgArg.SetValue(graph, OutcomeMethod.Args, args);

            if (CanAutoComplete)
            {
                if (CostMethod.Execute(graph))
                    graph.Time.Refresh();

                if (PerformMethod.Execute(graph))
                    graph.Time.Refresh();

                if (OutcomeMethod.Execute(graph))
                    graph.Time.Refresh();
            }

            Complete();
        }

        public void Reset()
        {
            CostMethod.Reset(this);
            PerformMethod.Reset(this);
            OutcomeMethod.Reset(this);

            RecommendedActions.Clear();
            IsComplete = false;
        }
    }
}
