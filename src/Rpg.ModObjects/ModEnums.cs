using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects
{
    public enum ModExpiry
    {
        Active,
        Pending,
        Expired
    }

    public enum ModType
    {
        Base,
        BaseOverride,
        Transient,
        State
    }

    public enum ModAction
    {
        Replace,
        Sum,
        Accumulate
    }

    public enum ModDurationType
    {
        Permanent,
        Timed,
        OnNewTurn,
        EndOfEncounter,
        OnValueZero,
        External
    }
}
