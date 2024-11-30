using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Activities
{
    public enum ActionEntity
    {
        Owner,
        Initiator,
        Activity
    }

    public enum ActionStatus
    {
        NotStarted,
        CanAutoComplete,
        Started,
        CanComplete,
        Completed
    }

    public enum RpgActionArgType
    {
        Any,
        Actor,
        Roll,
        Range,
        Cover
    }
}
