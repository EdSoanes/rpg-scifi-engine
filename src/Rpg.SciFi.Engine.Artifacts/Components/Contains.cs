using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class Contains
    {
        [JsonProperty] public Artifact[] Contents { get; protected set; } = new Artifact[0];
        [JsonProperty] public int MaxContents { get; protected set; } = 0;
    }
}
