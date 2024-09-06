using Rpg.ModObjects.States;
using System.Text.Json.Serialization;

namespace Rpg.Cyborgs.States
{
    public class Parrying : State<Actor>
    {
        [JsonConstructor] private Parrying() { }

        public Parrying(Actor owner)
            : base(owner) { }
    }
}
