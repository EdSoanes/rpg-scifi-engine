using Newtonsoft.Json;
using Rpg.Experimental.Graph;
using Rpg.Experimental.ModSets;
using Rpg.Experimental.Reflection.Args;

namespace Rpg.Experimental.Activities
{
    public sealed class RpgActivityAction : RpgObject
    {
        private RpgActivity? _activity;
        private RpgAction? _action;

        [JsonIgnore] public ModSet Outcome { get; private set; }
        [JsonProperty] public string ActionId { get; private set; }
        [JsonProperty] public string ActionOwnerId { get; private set; }

        [JsonProperty] public RpgActivityActionMethod CostMethod = new RpgActivityActionMethod();

        [JsonProperty] public RpgActivityActionMethod PerformMethod = new RpgActivityActionMethod();

        [JsonProperty] public RpgActivityActionMethod OutcomeMethod = new RpgActivityActionMethod();

        [JsonProperty] public List<string> RecommendedActions { get; private set; } = new();

        [JsonProperty] public bool IsComplete { get; private set; }

        public bool CanAutoComplete { get => _action != null && !IsComplete && AllStepArgsComplete; }
        public bool AllStepsComplete { get => CostMethod.IsDone && PerformMethod.IsDone && OutcomeMethod.IsDone; }
        public bool AllStepArgsComplete { get => CostMethod.Args.IsComplete() && PerformMethod.Args.IsComplete() && OutcomeMethod.Args.IsComplete(); }

        [JsonConstructor] private RpgActivityAction() { }

        public RpgActivityAction(RpgActivity owner, RpgAction action)
            : base(owner.Id, true)
        {
            _action = action;
            _activity = owner;

            ActionId = action.Id;
            ActionOwnerId = action.OwnerId!;
        }

        public void Reset(string methodName)
        {
            OutcomeMethod.Reset(this);
            if (methodName == MethodNames.Perform)
                PerformMethod.Reset(this);

            if (methodName == MethodNames.Cost)
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

            CostMethod.OnCreating(this, MethodNames.Cost);
            PerformMethod.OnCreating(this, MethodNames.Perform);
            OutcomeMethod.OnCreating(this, MethodNames.Outcome);

            RestoreOutcome(graph);
        }

        public override void OnRestoring(RpgGraph graph)
        {
            base.OnRestoring(graph);

            _activity = graph.GetObject(OwnerId) as RpgActivity;
            _action = graph.GetObject(ActionId) as RpgAction;

            CostMethod.OnRestoring(this);
            PerformMethod.OnRestoring(this);
            OutcomeMethod.OnRestoring(this);

            RestoreOutcome(graph);
        }

        public override void OnTimeEvent(RpgGraph graph)
        {
            base.OnTimeEvent(graph);

            CostMethod.OnTimeEvent(graph, this);
            PerformMethod.OnTimeEvent(graph, this);
            OutcomeMethod.OnTimeEvent(graph, this);
        }

        public override RpgObject? ResolvePropertyNameToObject(RpgGraph graph, string prop)
        {
            RpgObject? obj = prop switch
            {
                ReservedArgs.Context => graph.Context,
                ReservedArgs.Owner => graph.GetObject(ActionOwnerId),
                ReservedArgs.Initiator => graph.Actor,
                ReservedArgs.Action => graph.GetObject(ActionId),
                ReservedArgs.Activity => graph.GetObject(OwnerId),
                ReservedArgs.ActivityAction => this,
                _ => null
            };

            return obj;
        }

        private void RestoreOutcome(RpgGraph graph)
        {
            if (Outcome == null)
            {
                var outcome = graph.GetOwnerModSets(Id)?.FirstOrDefault(x => x.Name == MethodNames.Outcome);
                if (outcome == null)
                {
                    outcome = new ModSet(MethodNames.Outcome, Id, false)
                        .Lifespan(Start, End);

                    outcome.Unapply();
                    graph.Add(outcome);
                }

                Outcome = outcome;
            }
        }

        public void Complete()
        {
            if (AllStepsComplete)
            {
                Outcome.Apply();
                IsComplete = true;
            }
        }

        public void AutoComplete(RpgGraph graph, params (string, object?)[]? args)
        {
            CostMethod.Args.Set(args);
            PerformMethod.Args.Set(args);
            OutcomeMethod.Args.Set(args);

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
            Outcome.Reset();
            RecommendedActions.Clear();
            IsComplete = false;
        }
    }
}
