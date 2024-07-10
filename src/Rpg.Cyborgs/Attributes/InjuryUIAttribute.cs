using Rpg.ModObjects.Meta;
using Rpg.ModObjects.Meta.Props;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Cyborgs.Attributes
{
    public class InjuryUIAttribute : MetaSelectAttribute
    {
        public InjuryUIAttribute()
            : base("None", "Flesh Wound", "Unusable", "Busted", "Mangled", "Severed/Eviscerated", "Obliterated")
        {
            Editor = EditorType.Select;
            DataTypeName = "Injury";
        }
    }
}
