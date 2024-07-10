using Newtonsoft.Json;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Time.Lifecycles;

namespace Rpg.ModObjects.Actions
{
    public class ActionInstance
    {
        [JsonProperty] public string OwnerId { get; protected set; }
        [JsonProperty] public string InitiatorId { get; protected set; }
        [JsonProperty] public string ActionName { get; protected set; }
        [JsonProperty] public int ActionNo { get; protected set; }

        [JsonIgnore] public RpgArgSet? CanActArgs {  get; protected set; }
        [JsonIgnore] public RpgArgSet? CostArgs { get; protected set; }
        [JsonIgnore] public RpgArgSet? ActArgs { get; protected set; }
        [JsonIgnore] public RpgArgSet? OutcomeArgs { get; protected set; }
        [JsonIgnore] public RpgArgSet? AutoCompleteArgs { get; protected set; }

        public ActionInstance(RpgEntity owner, RpgEntity initiator, Action action, int actionNo)
        {
            ActionName = action.Name;
            ActionNo = actionNo;
            OwnerId = owner.Id;
            InitiatorId = initiator.Id;
        }

        public void OnBeforeTime(RpgGraph graph)
        {
            var initiator = graph.GetEntity<RpgEntity>(InitiatorId)!;
            var owner = graph.GetEntity<RpgEntity>(OwnerId)!;
            var action = owner.GetAction(ActionName)!;

            CanActArgs = action.CanActArgs();
            CostArgs = action.CostArgs();
            ActArgs = action.ActArgs();
            OutcomeArgs = action.OutcomeArgs();
            AutoCompleteArgs = CanActArgs
                .Merge(CostArgs)
                .Merge(ActArgs)
                .Merge(OutcomeArgs);

            if (CostArgs.HasArg("actionInstance"))
                CostArgs["actionInstance"] = this;

            if (CostArgs.HasArg("actionNo"))
                CostArgs["actionNo"] = ActionNo;

            if (CostArgs.HasArg("initiator"))
                CostArgs["initiator"] = initiator;

            if (CostArgs.HasArg("owner"))
                CostArgs["owner"] = owner;


            if (ActArgs.HasArg("actionInstance"))
                ActArgs["actionInstance"] = this;

            if (ActArgs.HasArg("actionNo"))
                ActArgs["actionNo"] = ActionNo;

            if (ActArgs.HasArg("initiator"))
                ActArgs["initiator"] = initiator;

            if (ActArgs.HasArg("owner"))
                ActArgs["owner"] = owner;


            if (OutcomeArgs.HasArg("actionInstance"))
                OutcomeArgs["actionInstance"] = this;

            if (OutcomeArgs.HasArg("actionNo"))
                OutcomeArgs["actionNo"] = ActionNo;

            if (OutcomeArgs.HasArg("initiator"))
                OutcomeArgs["initiator"] = initiator;

            if (OutcomeArgs.HasArg("owner"))
                OutcomeArgs["owner"] = owner;


            if (AutoCompleteArgs.HasArg("actionNo"))
                AutoCompleteArgs["actionNo"] = ActionNo;

            if (AutoCompleteArgs.HasArg("initiator"))
                AutoCompleteArgs["initiator"] = initiator;

            if (AutoCompleteArgs.HasArg("owner"))
                AutoCompleteArgs["owner"] = owner;
        }

        public ActionModSet CreateActionSet()
            => new ActionModSet(InitiatorId, ActionName, ActionNo);

        public OutcomeModSet CreateOutcomeSet()
            => new OutcomeModSet(InitiatorId, ActionName, ActionNo);

        public bool CanAct(RpgGraph graph)
            => graph.GetEntity<RpgEntity>(OwnerId)!
                .GetAction(ActionName)!
                .CanAct(CanActArgs!);

        public ModSet Cost(RpgGraph graph)
            => graph.GetEntity<RpgEntity>(OwnerId)!
                .GetAction(ActionName)!
                .Cost(CostArgs!);

        public ActionModSet Act(RpgGraph graph)
            => graph.GetEntity<RpgEntity>(OwnerId)!
                .GetAction(ActionName)!
                .Act(ActArgs!);

        public ModSet[] Outcome(RpgGraph graph)
            => graph.GetEntity<RpgEntity>(OwnerId)!
                .GetAction(ActionName)!
                .Outcome(OutcomeArgs!);

        public void AutoComplete(RpgGraph graph)
        {
            if (CanActArgs == null)
                OnBeforeTime(graph);

            CanActArgs!.FillFrom(AutoCompleteArgs!);
            if (!CanAct(graph))
                throw new InvalidOperationException("Cannot AutoComplete");

            CostArgs!.FillFrom(AutoCompleteArgs!);
            var costs = Cost(graph);
            var entity = graph.GetEntity(costs.OwnerId)!;
            entity.AddModSet(costs);
            graph.Time.TriggerEvent();

            ActArgs!.FillFrom(AutoCompleteArgs!);
            var actionModSet = Act(graph);
            
            var owner = graph.GetEntity(actionModSet.OwnerId)!;
            owner.AddModSet(actionModSet);
            graph.Time.TriggerEvent();

            OutcomeArgs!.FillFrom(AutoCompleteArgs!);
            var outcomeSets = Outcome(graph);
            foreach (var outcomeSet in outcomeSets)
            {
                owner = graph.GetEntity(outcomeSet.OwnerId)!;
                owner.AddModSet(outcomeSet);
                graph.Time.TriggerEvent();
            }
        }
    }
}
