using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.MetaData;
using Rpg.SciFi.Engine.Artifacts.Modifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class Contains : Entity
    {
        public Contains(int baseEncumbrance = int.MaxValue)
        {
            BaseEncumbrance = baseEncumbrance;
        }

        [JsonProperty] protected List<Artifact> Artifacts { get; set; } = new List<Artifact>();
        [JsonProperty] public int BaseEncumbrance { get; protected set; }

        [Moddable] public int Encumbrance { get => this.Resolve(nameof(Encumbrance)); }

        [Setup]
        public void Setup()
        {
            this.Mod((x) => x.BaseEncumbrance, (x) => x.Encumbrance).IsBase().Apply();
        }

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
