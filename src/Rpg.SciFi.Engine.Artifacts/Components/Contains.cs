using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class Contains : Modifiable
    {
        public Contains(int baseEncumbrance = int.MaxValue)
        {
            BaseEncumbrance = baseEncumbrance;
        }

        [JsonProperty] protected List<Artifact> Artifacts { get; set; } = new List<Artifact>();
        [JsonProperty] public int BaseEncumbrance { get; protected set; }

        [Moddable] public int Encumbrance { get => BaseEncumbrance + ModifierRoll(nameof(Encumbrance)); }

        public void Add(Artifact artifact)
        {
            if (Artifacts.Any(x => x.Id == artifact.Id))
                throw new ArgumentException($"Artifact {artifact.Id} {artifact.Name} exists in container");

            if (Artifacts.Sum(x => x.Weight) + artifact.Weight > Encumbrance)
                throw new ArgumentException($"Adding artifact {artifact.Id} {artifact.Name} would exceed encumbrance");

            Artifacts.Add(artifact);
        }

        public void Remove(Guid id)
        {
            if (!Artifacts.Any(x => x.Id == id))
                throw new ArgumentException($"Artifact {id} does not exist in container");
            
            var toRemove = Artifacts.Single(x => x.Id == id);
            Artifacts.Remove(toRemove);
        }

        public void Clear()
        {
            Artifacts.Clear();
        }
    }
}
