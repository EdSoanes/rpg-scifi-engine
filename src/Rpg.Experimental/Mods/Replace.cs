using Newtonsoft.Json;
using Rpg.Experimental.Graph;

namespace Rpg.Experimental.Mods
{
    public class Replace : Mod
    {
        public Replace()
            : base(ModType.Standard, ModBehavior.Replace)
        { }

        public Replace(RpgPropertyRef? target)
            : this()
        {
            SetTarget(target);
        }

        public Replace(RpgPropertyRef target, RpgPropertyRef source)
            : this()
        {
            SetTarget(target);
            SetSource(source);
        }

        public Replace(RpgObject target, string targetProp)
            : this()
        {
            SetTarget(new RpgPropertyRef(target.Id, targetProp));
        }

        public Replace(RpgObject target, string targetProp, Dice dice)
            : this()
        {
            SetTarget(new RpgPropertyRef(target.Id, targetProp));
            SetSource(dice);
        }

        public Replace(RpgObject target, string targetProp, RpgObject source, string sourceProp)
            : this(new RpgPropertyRef(target.Id, targetProp), new RpgPropertyRef(source.Id, sourceProp))
        { }
    }
}
