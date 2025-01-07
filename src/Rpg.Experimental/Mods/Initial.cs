using Newtonsoft.Json;
using Rpg.Experimental.Graph;

namespace Rpg.Experimental.Mods
{
    public class Initial : Replace
    {
        [JsonConstructor] private Initial()
            : base(ModType.Initial)
        {
        }

        public Initial(RpgPropertyRef target, Dice dice)
            : this()
        {
            SetTarget(target);
            SetSource(dice);
        }
    }
}
