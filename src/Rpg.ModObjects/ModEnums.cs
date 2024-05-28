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

    //public enum ModType
    //{
    //    BaseInit,
    //    Base,
    //    BaseOverride,
    //    Permanent,
    //    Transient,
    //    State
    //}

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
        OnValueZero,
        External
    }
}
