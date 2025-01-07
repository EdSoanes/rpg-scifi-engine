using Rpg.Experimental.Graph;

namespace Rpg.Experimental.Mods
{
    public class Override : Mod
    {
        public Override()
            : base(ModType.Override, ModBehavior.Replace)
        { }

        public Override(RpgPropertyRef? target)
            : this()
        {
            SetTarget(target);
        }

        public Override(RpgPropertyRef target, RpgPropertyRef source)
            : this()
        {
            SetTarget(target);
            SetSource(source);
        }

        public Override(RpgObject target, string targetProp)
            : this()
        {
            SetTarget(new RpgPropertyRef(target.Id, targetProp));
        }

        public Override(RpgObject target, string targetProp, Dice dice)
            : this()
        {
            SetTarget(new RpgPropertyRef(target.Id, targetProp));
            SetSource(dice);
        }

        public Override(RpgObject target, string targetProp, RpgObject source, string sourceProp)
            : this(new RpgPropertyRef(target.Id, targetProp), new RpgPropertyRef(source.Id, sourceProp))
        { }
    }
}
