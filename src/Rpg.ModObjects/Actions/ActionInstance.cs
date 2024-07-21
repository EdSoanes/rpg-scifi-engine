using Newtonsoft.Json;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Reflection;

namespace Rpg.ModObjects.Actions
{
    public class ActionInstance
    {
        private RpgGraph? Graph { get; set; }

        private RpgEntity? _owner;
        private RpgEntity? Owner 
        { 
            get
            {
                if (_owner == null && Graph != null)
                    _owner = Graph.GetObject<RpgEntity>(OwnerId);

                return _owner;
            }
        }

        private RpgEntity? _initiator;
        private RpgEntity? Initiator
        {
            get
            {
                if (_initiator == null && Graph != null)
                    _initiator = Graph.GetObject<RpgEntity>(InitiatorId);

                return _initiator;
            }
        }

        private Action? _action;
        private Action? Action
        {
            get
            {
                if (_action == null && Graph != null)
                    _action = Owner?.GetAction(ActionName);

                return _action;
            }
        }

        [JsonProperty] public string OwnerId { get; protected set; }
        [JsonProperty] public string InitiatorId { get; protected set; }
        [JsonProperty] public string ActionName { get; protected set; }
        [JsonProperty] public int ActionNo { get; protected set; }

        [JsonProperty] public RpgArgSet? CanActArgs {  get; protected set; }
        [JsonProperty] public RpgArgSet? CostArgs { get; protected set; }
        [JsonProperty] public RpgArgSet? ActArgs { get; protected set; }
        [JsonProperty] public RpgArgSet? OutcomeArgs { get; protected set; }
        [JsonProperty] public RpgArgSet? AutoCompleteArgs { get; protected set; }

        public ActionInstance(RpgEntity owner, RpgEntity initiator, Action action, int actionNo)
        {
            ActionName = action.Name;
            ActionNo = actionNo;
            OwnerId = owner.Id;
            InitiatorId = initiator.Id;
        }

        public void OnBeforeTime(RpgGraph graph)
        {
            Graph = graph;

            if (Action == null)
                throw new InvalidOperationException("ActionInstance OnBeforeTime could not find Action");

            CanActArgs = Action.CanActArgs();
            CostArgs = Action.CostArgs();
            ActArgs = Action.ActArgs();
            OutcomeArgs = Action.OutcomeArgs();
            AutoCompleteArgs = CanActArgs
                .Merge(CostArgs)
                .Merge(ActArgs)
                .Merge(OutcomeArgs);

            CanActArgs.SetArgValues(this, Owner, Initiator, ActionNo);
            CostArgs.SetArgValues(this, Owner, Initiator, ActionNo);
            ActArgs.SetArgValues(this, Owner, Initiator, ActionNo);
            OutcomeArgs.SetArgValues(this, Owner, Initiator, ActionNo);
            AutoCompleteArgs.SetArgValues(this, Owner, Initiator, ActionNo);
        }

        public ActionModSet CreateActionSet()
            => new ActionModSet(InitiatorId, ActionName, ActionNo);

        public OutcomeModSet CreateOutcomeSet()
            => new OutcomeModSet(InitiatorId, ActionName, ActionNo);

        public bool CanAct()
            => Graph.GetObject<RpgEntity>(OwnerId)!
                .GetAction(ActionName)!
                .CanAct(CanActArgs!);

        public ModSet Cost()
            => Graph.GetObject<RpgEntity>(OwnerId)!
                .GetAction(ActionName)!
                .Cost(CostArgs!);

        public ActionModSet Act()
            => Graph.GetObject<RpgEntity>(OwnerId)!
                .GetAction(ActionName)!
                .Act(ActArgs!);

        public ModSet[] Outcome()
            => Graph.GetObject<RpgEntity>(OwnerId)!
                .GetAction(ActionName)!
                .Outcome(OutcomeArgs!);

        public void SetArgValue(string arg, object? value)
        {
            CanActArgs!.SetArg(arg, value);
            CostArgs!.SetArg(arg, value);
            ActArgs!.SetArg(arg, value);
            OutcomeArgs!.SetArg(arg, value);
            AutoCompleteArgs!.SetArg(arg, value);
        }

        public void SetArgValues(Dictionary<string, string?> argValues)
        {
            CanActArgs!.SetArgValues(argValues);
            CostArgs!.SetArgValues(argValues);
            ActArgs!.SetArgValues(argValues);
            OutcomeArgs!.SetArgValues(argValues);
            AutoCompleteArgs!.SetArgValues(argValues);
        }

        public void AutoComplete()
        {
            if (CanActArgs == null)
                OnBeforeTime(Graph!);

            CanActArgs!.FillFrom(AutoCompleteArgs!);
            if (!CanAct())
                throw new InvalidOperationException("Cannot AutoComplete");

            CostArgs!.FillFrom(AutoCompleteArgs!);
            var costs = Cost();
            var entity = Graph!.GetObject(costs.OwnerId)!;
            entity.AddModSet(costs);
            Graph!.Time.TriggerEvent();

            ActArgs!.FillFrom(AutoCompleteArgs!);
            var actionModSet = Act();
            
            var owner = Graph!.GetObject(actionModSet.OwnerId)!;
            owner.AddModSet(actionModSet);
            Graph!.Time.TriggerEvent();

            OutcomeArgs!.FillFrom(AutoCompleteArgs!);
            var outcomeSets = Outcome();
            foreach (var outcomeSet in outcomeSets)
            {
                owner = Graph!.GetObject(outcomeSet.OwnerId)!;
                owner.AddModSet(outcomeSet);
                Graph!.Time.TriggerEvent();
            }
        }
    }
}
