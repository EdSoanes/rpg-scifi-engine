using Newtonsoft.Json;
using Rpg.Sys.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Archetypes
{
    public class Equipment : Artifact
    {
        [JsonConstructor] private Equipment() { }
        
        public Equipment(ArtifactTemplate template)
            : base(template)
        {
        }
    }
}
