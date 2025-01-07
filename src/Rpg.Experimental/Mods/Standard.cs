using Rpg.Experimental.Graph;

namespace Rpg.Experimental.Mods
{
    public class Standard : Mod
    {
        public Standard()
            : base()
        { }

        public Standard(RpgPropertyRef? target)
            : this()
        {
            SetTarget(target);
        }

        public Standard(RpgPropertyRef target, RpgPropertyRef source)
            : this()
        {
            SetTarget(target);
            SetSource(source);
        }

        public Standard(RpgObject target, string targetProp)
            : this()
        {
            SetTarget(new RpgPropertyRef(target.Id, targetProp));
        }

        public Standard(RpgObject target, string targetProp, Dice dice)
            : this()
        {
            SetTarget(new RpgPropertyRef(target.Id, targetProp));
            SetSource(dice);
        }

        public Standard(RpgObject target, string targetProp, RpgObject source, string sourceProp)
            : this(new RpgPropertyRef(target.Id, targetProp), new RpgPropertyRef(source.Id, sourceProp))
        { }
    }
}
