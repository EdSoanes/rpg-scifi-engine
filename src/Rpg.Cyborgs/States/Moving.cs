using Rpg.ModObjects.States;
using System.Text.Json.Serialization;

namespace Rpg.Cyborgs.States
{
    public class Moving : State<Actor>
    {
        [JsonConstructor] private Moving() { }

        public Moving(Actor owner)
            : base(owner) { }
    }
}
