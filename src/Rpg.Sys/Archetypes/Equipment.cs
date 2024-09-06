using System.Text.Json.Serialization;

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
