using Rpg.ModObjects.States;
using Newtonsoft.Json;

namespace Rpg.Cyborgs.States
{
    public class Moving : State<Actor>
    {
        [JsonConstructor] private Moving() { }

        public Moving(Actor owner)
            : base(owner) { }
    }
}
