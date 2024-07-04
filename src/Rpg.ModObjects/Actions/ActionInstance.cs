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
        
        public RpgArgSet CostArgs { get; protected set; }
        public RpgArgSet ActArgs { get; protected set; }
        public RpgArgSet OutcomeArgs { get; protected set; }

        public Dice ActResult { get => Action.ActResult(Initiator, ActionNo); }
        public Dice OutcomeResult { get => Action.OutcomeResult(Initiator, ActionNo); }

        public abstract bool CanAct();

        public ModSet Cost()
            => Action.Cost(CostArgs);

        public ModSet[] Act()
            => Action.Act(ActArgs);

        public ModSet[] Outcome()
            => Action.Outcome(OutcomeArgs);
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

            CostArgs = action.CostArgs();
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

            OutcomeArgs = action.OutcomeArgs();
            if (OutcomeArgs.HasArg("actionNo"))
                OutcomeArgs["actionNo"] = actionNo;

            if (OutcomeArgs.HasArg("initiator"))
                OutcomeArgs["initiator"] = initiator;

            if (OutcomeArgs.HasArg("owner"))
                OutcomeArgs["owner"] = owner;
        }

        public override bool CanAct()
            => Action.IsEnabled(Owner, Initiator);
    }
}
