using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine
{
    public abstract class ConsumableArtifact : Artifact
    {
        public string ContentType { get; set; } = string.Empty;
        public int Max { get; set; }
        public int Current { get; set; }

        public ConsumableArtifact()
        {
        }

        public bool CanRefill(int count) => Current + count <= Max;

        public void Refill(int count)
        {
            if (CanRefill(count))
                Current += count;
        }

        public bool CanConsume(int count) => Current - count >= 0;

        public void Consume(int count)
        {
            if (CanConsume(count))
                Current -= count;
        }
    }
}
