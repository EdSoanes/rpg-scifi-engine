using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts
{
    public abstract class Entity
    {
        [JsonProperty] public Guid Id { get; private set; } = Guid.NewGuid();
    }
}
