using Newtonsoft.Json;
using Rpg.Sys.Components;

namespace Rpg.Sys.Archetypes
{
    public class Equipment : Artifact
    {
        [JsonConstructor] protected Equipment() { }
        
        public Equipment(ArtifactTemplate template)
            : base(template)
        {
            States.Add(
                new OnState(Id)
            );
        }
    }
}
