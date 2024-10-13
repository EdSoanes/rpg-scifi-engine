using Rpg.ModObjects.States;
using Rpg.Sys.Archetypes;
using Newtonsoft.Json;

namespace Rpg.Sys.States
{
    public class Moving : State<Actor>
    {
        [JsonConstructor] private Moving() { }

        public Moving(Actor owner)
            : base(owner) { }
    }
}
