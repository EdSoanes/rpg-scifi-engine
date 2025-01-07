using Rpg.Experimental.Graph;

namespace Rpg.Experimental.Mods
{
    public class Base : Mod
    {
        public Base()
            : base(ModType.Base)
        {
        }

        public Base(RpgPropertyRef? target)
            : this()
        {
            SetTarget(target);
        }

        public Base(RpgPropertyRef target, RpgPropertyRef source)
            : this()
        {
            SetTarget(target);
            SetSource(source);
        }

        public Base(RpgObject target, string targetProp)
            : this()
        {
            SetTarget(new RpgPropertyRef(target.Id, targetProp));
        }

        public Base(RpgObject target, string targetProp, Dice dice)
            : this()
        {
            SetTarget(new RpgPropertyRef(target.Id, targetProp));
            SetSource(dice);
        }

        public Base(RpgObject target, string targetProp, RpgObject source, string sourceProp)
            : this(new RpgPropertyRef(target.Id, targetProp), new RpgPropertyRef(source.Id, sourceProp))
        { }
    }
}
