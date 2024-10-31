using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Time
{
    public enum LifecycleExpiry
    {
        Unset,
        Pending,
        Active,
        Expired,
        Destroyed
    }

    public enum PointInTimeType
    {
        BeforeTime,
        TimeBegins,
        Waiting,

        EncounterBegins,
        Turn, //Count
        EncounterEnds,
        //MinutePasses,
        TimePasses,
        //DayPasses,

        TimeEnds
    }
}
