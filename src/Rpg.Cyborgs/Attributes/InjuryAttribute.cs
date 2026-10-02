using Rpg.Experimental.System.Props;

namespace Rpg.Cyborgs.Attributes
{
    public class InjuryAttribute : SelectAttribute
    {
        public InjuryAttribute()
            : base("None", "Flesh Wound", "Unusable", "Busted", "Mangled", "Severed/Eviscerated", "Obliterated")
        {
        }
    }
}
