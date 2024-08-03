using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Cyborgs
{
    public enum InjuryLocationType
    {
        Random = 0,
        High = 1,
        Low = 2
    };

    public enum BodyPartType
    {
        Limb,
        Torso,
        Head
    }

    public enum InjurySeverityEnum
    {
        None = 0,
        FleshWound = 1,
        Unusable = 2,
        Busted = 3,
        Mangled = 4,
        Severed = 5,
        Obliterated = 6
    }
}
