using Newtonsoft.Json;

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
