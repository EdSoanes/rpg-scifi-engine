using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Moddable
{
    public enum ModExpiry
    {
        Active,
        Remove,
        Disabled,
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
        OnTurn,
        EndOfTurn,
        EndOfEncounter,
        WhenPropertyZero
    }

}
