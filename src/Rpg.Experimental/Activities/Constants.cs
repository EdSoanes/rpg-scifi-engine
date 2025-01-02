using Rpg.Experimental.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Experimental.Activities
{
    public class MethodNames
    {
        public const string CanPerform = "CanPerform";
        public const string Cost = "Cost";
        public const string Perform = "Perform";
        public const string Outcome = "Outcome";
    }

    public class ReservedArgs
    {
        public const string Owner = "owner";
        public const string Initiator = "initiator";
        public const string Action = "action";
        public const string Activity = "activity";
        public const string ActivityAction = "activityAction";
        public const string Context = "context";
        public const string Graph = "graph";

        public Type Type(string argName)
            => argName switch
            {
                Owner => typeof(RpgObject),
                Initiator => typeof(RpgObject),
                Action => typeof(RpgAction),
                //Activity => typeof(RpgActivity),
                //ActivityAction => typeof(RpgActivityAction),
                Graph => typeof(RpgGraph),
                _ => typeof(Dice)
            };
    }
}
