using Rpg.Experimental.Graph;

namespace Rpg.Experimental.Mods
{
    public class Combine : Mod
    {
        public Combine()
            : base(ModType.Standard)
        { }

        public Combine(RpgPropertyRef? target)
            : this()
        {
            SetTarget(target);
        }

        public Combine(RpgPropertyRef target, RpgPropertyRef source)
            : this()
        {
            SetTarget(target);
            SetSource(source);
        }

        public Combine(RpgObject target, string targetProp)
            : this()
        {
            SetTarget(new RpgPropertyRef(target.Id, targetProp));
        }

        public Combine(RpgObject target, string targetProp, Dice dice)
            : this()
        {
            SetTarget(new RpgPropertyRef(target.Id, targetProp));
            SetSource(dice);
        }

        public Combine(RpgObject target, string targetProp, RpgObject source, string sourceProp)
            : this(new RpgPropertyRef(target.Id, targetProp), new RpgPropertyRef(source.Id, sourceProp))
        { }
    }
}
