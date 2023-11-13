using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Archetypes
{
    public interface IContainerArtifact
    {
        List<Artifact> Contents { get; }
        public int Capacity { get; }
    }
}
