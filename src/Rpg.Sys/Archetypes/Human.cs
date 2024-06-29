using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.Sys.Components;

namespace Rpg.Sys.Archetypes
{
    public class Human : Actor
    {
        [JsonConstructor] protected Human() { }

        public Human(ActorTemplate template)
            : base(template) 
        { }
    }
}
