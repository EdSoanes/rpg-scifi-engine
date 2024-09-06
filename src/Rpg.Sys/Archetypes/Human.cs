using System.Text.Json.Serialization;

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
