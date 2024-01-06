using Newtonsoft.Json;
using Rpg.Sys.Components;

namespace Rpg.Sys.Archetypes
{
    public class Equipment : Artifact
    {
        [JsonConstructor] private Equipment() { }
        
        public Equipment(ArtifactTemplate template)
            : base(template)
        {
            States.Add(
                new OnState(Id)
            );
        }
    }

    public class OnState : State<Equipment> 
    {
        public OnState(Guid id) 
            : base(id, "On") 
        { 
        }
    }
}
