using Rpg.ModObjects.Meta.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Cyborgs.Attributes
{
    public class InjuryUIAttribute : SelectUIAttribute
    {
        public InjuryUIAttribute()
            : base("None", "Flesh Wound", "Unusable", "Busted", "Mangled", "Severed/Eviscerated", "Obliterated")
        {
            DataType = "Select";
            DataTypeName = "Injury";
        }
    }
}
