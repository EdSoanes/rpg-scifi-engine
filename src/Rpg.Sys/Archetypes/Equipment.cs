using Newtonsoft.Json;

namespace Rpg.Sys.Archetypes
{
    public class Equipment : Artifact
    {
        [JsonConstructor] protected Equipment() { }
        
        public Equipment(ArtifactTemplate template)
            : base(template)
        {
        }
    }
}
