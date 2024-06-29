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

        public override void OnBeforeTime(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnBeforeTime(graph, this);
            Stats.OnBeforeTime(graph, this);
            Actions.OnBeforeTime(graph, this);
            Damage.OnBeforeTime(graph, this);
            Defenses.OnBeforeTime(graph, this); 
            Movement.OnBeforeTime(graph, this);
            Presence.OnBeforeTime(graph, this);
        }
    }
}
