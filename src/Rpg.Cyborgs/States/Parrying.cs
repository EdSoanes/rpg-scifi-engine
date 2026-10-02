using Rpg.Experimental;
using Newtonsoft.Json;

namespace Rpg.Cyborgs.States
{
    public class Parrying : RpgState<Actor>
    {
        [JsonConstructor] private Parrying() { IsPlayerVisible = false; }

        public Parrying(Actor owner)
            : base(owner) { IsPlayerVisible = false; }
    }
}
