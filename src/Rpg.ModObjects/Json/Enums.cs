using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Json
{
    public enum Ops
    {
        NoOp,
        Insert,
        Update,
        Delete
    };

    public enum PropType
    {
        Source,
        Target
    }
}
