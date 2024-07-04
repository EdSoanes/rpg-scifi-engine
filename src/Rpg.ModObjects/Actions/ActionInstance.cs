using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Actions
{
    public abstract class ActionInstance<TInitiator>
        where TInitiator : RpgEntity
    {
        public int ActionNo { get; protected set; }
        public TInitiator Initiator { get; protected set; }
        public Action Action { get; protected set; }
        
        public RpgArgSet CanActArgs {  get; protected set; } 
        public RpgArgSet CostArgs { get; protected set; }
        public RpgArgSet ActArgs { get; protected set; }
        public RpgArgSet OutcomeArgs { get; protected set; }
        public RpgArgSet AutoCompleteArgs { get; protected set; }

        public Dice ActResult { get => Action.ActResult(Initiator, ActionNo); }
        public Dice OutcomeResult { get => Action.OutcomeResult(Initiator, ActionNo); }

        public bool CanAct()
            => Action.CanAct(CanActArgs);

        public ModSet Cost()
            => Action.Cost(CostArgs);

        public ModSet[] Act()
            => Action.Act(ActArgs);

        public ModSet[] Outcome()
            => Action.Outcome(OutcomeArgs);

        public void AutoComplete(RpgGraph graph)
        {
            CanActArgs.FillFrom(AutoCompleteArgs);
            if (!CanAct())
                throw new InvalidOperationException("Cannot AutoComplete");

            CostArgs.FillFrom(AutoCompleteArgs);
            var costs = Cost();
            var entity = graph.GetEntity(costs.OwnerId)!;
            entity.AddModSet(costs);
            graph.Time.TriggerEvent();

            ActArgs.FillFrom(AutoCompleteArgs);
            var actSets = Act();
            foreach (var actSet in  actSets)
            {
                var owner = graph.GetEntity(actSet.OwnerId)!;
                owner.AddModSet(actSet);
                graph.Time.TriggerEvent();
            }

            OutcomeArgs.FillFrom(AutoCompleteArgs);
            var outcomeSets = Outcome();
            foreach (var outcomeSet in outcomeSets)
            {
                var owner = graph.GetEntity(outcomeSet.OwnerId)!;
                owner.AddModSet(outcomeSet);
                graph.Time.TriggerEvent();
            }
        }
    }

    public sealed class ActionInstance<TOwner, TInitiator> : ActionInstance<TInitiator>
        where TOwner: RpgEntity
        where TInitiator : RpgEntity
    {
        public TOwner Owner { get; private set; }

        internal ActionInstance(TOwner owner, TInitiator initiator, Action action, int actionNo)
        {
            ActionNo = actionNo;
            Owner = owner;
            Initiator = initiator;
            Action = action;

            CanActArgs = action.CanActArgs();
            CostArgs = action.CostArgs();
            ActArgs = action.ActArgs();
            OutcomeArgs = action.OutcomeArgs();
            AutoCompleteArgs = CanActArgs
                .Merge(CostArgs)
                .Merge(ActArgs)
                .Merge(OutcomeArgs);

            if (CostArgs.HasArg("actionNo"))
                CostArgs["actionNo"] = actionNo;

            if (CostArgs.HasArg("initiator"))
                CostArgs["initiator"] = initiator;

            if (CostArgs.HasArg("owner"))
                CostArgs["owner"] = owner;


            ActArgs = action.ActArgs();
            if (ActArgs.HasArg("actionNo"))
                ActArgs["actionNo"] = actionNo;

            if (ActArgs.HasArg("initiator"))
                ActArgs["initiator"] = initiator;

            if (ActArgs.HasArg("owner"))
                ActArgs["owner"] = owner;


            if (OutcomeArgs.HasArg("actionNo"))
                OutcomeArgs["actionNo"] = actionNo;

            if (OutcomeArgs.HasArg("initiator"))
                OutcomeArgs["initiator"] = initiator;

            if (OutcomeArgs.HasArg("owner"))
                OutcomeArgs["owner"] = owner;


            if (AutoCompleteArgs.HasArg("actionNo"))
                AutoCompleteArgs["actionNo"] = actionNo;

            if (AutoCompleteArgs.HasArg("initiator"))
                AutoCompleteArgs["initiator"] = initiator;

            if (AutoCompleteArgs.HasArg("owner"))
                AutoCompleteArgs["owner"] = owner;
        }
    }
}
