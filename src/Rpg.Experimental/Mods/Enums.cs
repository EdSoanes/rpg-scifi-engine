using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Experimental.Mods
{
    public enum ModScope
    {
        Standard,
        ChildComponents,
        ChildObjects
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
        Synced,
        Threshold
    }

    public enum ModOwnerType
    {
        None,
        Object,
        ModSet,
        State,
        Action
    }
}
