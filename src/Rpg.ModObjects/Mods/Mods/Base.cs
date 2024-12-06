using Rpg.ModObjects.Values;

namespace Rpg.ModObjects.Mods.Mods
{
    public class Base : Mod
    {
        public Base()
            : base(nameof(Base))
        { }

        public Base(RpgObject target, string targetProp, Dice Value)
            : base(nameof(Base))
                => Set(target, targetProp, Value);

        public Base(string name, RpgObject target, string targetProp, Dice Value)
            : base(name)
                => Set(target, targetProp, Value);
    }
}
