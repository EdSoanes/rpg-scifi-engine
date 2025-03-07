using Newtonsoft.Json;
using Rpg.Experimental.Graph;

namespace Rpg.Experimental.Mods
{
    public class Initial : Mod
    {
        [JsonConstructor] private Initial()
            : base(ModType.Initial, ModBehavior.Replace)
        {
        }

        public Initial(RpgPropertyRef target, Dice dice)
            : this()
        {
            SetTarget(target);
            SetSource(dice);
        }

        public Initial(RpgObject obj, string prop, Dice dice)
            : this(new RpgPropertyRef(obj.Id, prop), dice)
        { }
    }
}
