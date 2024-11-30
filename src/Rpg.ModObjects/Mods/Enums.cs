using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Mods
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
        State,
        ForceState,
        Synced
    }
}
