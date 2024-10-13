using Rpg.ModObjects.States;
using Newtonsoft.Json;

namespace Rpg.Cyborgs.States
{
    public class Parrying : State<Actor>
    {
        [JsonConstructor] private Parrying() { }

        public Parrying(Actor owner)
            : base(owner) { }
    }
}
