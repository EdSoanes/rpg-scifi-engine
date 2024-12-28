using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Experimental.Time
{
    public enum LifecycleExpiry
    {
        Unset,
        Pending,
        Active,
        Suspended,
        Expired,
        Destroyed
    }

    public enum TimePointType
    {
        BeforeTime,
        TimeBegins,
        Waiting,
        EncounterBegins,
        Turn,
        EncounterEnds,
        TimePasses,
        TimeEnds
    }
}
