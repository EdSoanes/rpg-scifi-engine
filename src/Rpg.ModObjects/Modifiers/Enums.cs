using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Modifiers
{
    public enum ModExpiry
    {
        Active,
        Pending,
        Expired
    }

    public enum ModMerging
    {
        Add,
        Combine,
        Replace
    }

    public enum ModType
    {
        Initial,
        Base,
        Override,
        Standard,
        State,
        ForceState,
        Synced
    }

}
