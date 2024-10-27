using Rpg.ModObjects.States;
using Newtonsoft.Json;

namespace Rpg.Cyborgs.States
{
    public class Parrying : State<Actor>
    {
        [JsonConstructor] private Parrying() { IsPlayerVisible = false; }

        public Parrying(Actor owner)
            : base(owner) { IsPlayerVisible = false; }
    }
}
