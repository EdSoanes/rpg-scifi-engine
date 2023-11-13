using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rpg.SciFi.Engine.Archetypes;

namespace Rpg.SciFi.Engine
{
    public class Encounter : Artifact, IContainerArtifact
    {
        public List<Artifact> Contents => new List<Artifact>();

        public int Capacity => int.MaxValue;
    }
}
