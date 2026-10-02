using Rpg.Experimental;
using Newtonsoft.Json;

namespace Rpg.Cyborgs.States
{
    public class Moving : RpgState<Actor>
    {
        [JsonConstructor] private Moving() { IsPlayerVisible = false; }

        public Moving(Actor owner)
            : base(owner) { IsPlayerVisible = false; }
    }
}
