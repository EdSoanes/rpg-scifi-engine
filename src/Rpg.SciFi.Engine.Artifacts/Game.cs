using Rpg.SciFi.Engine.Artifacts.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts
{
    public class Game
    {
        public Character Character { get; set; }
        public Environment Environment { get; set; } = new Environment();
        public List<Character> Players { get; set; } = new List<Character>();

        public void Initialize()
        {
            MetaDiscovery.Initialize(this);
        }
    }
}
