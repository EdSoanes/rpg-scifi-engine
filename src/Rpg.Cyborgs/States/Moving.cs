using Newtonsoft.Json;
using Rpg.ModObjects.States;

namespace Rpg.Cyborgs.States
{
    public class Moving : State<Actor>
    {
        [JsonConstructor] private Moving() { }

        public Moving(Actor owner)
            : base(owner) { }
    }
}
