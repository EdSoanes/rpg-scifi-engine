using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.MetaData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts
{
    public class Game : Entity
    {
        public Character Character { get; set; }
        public Environment Environment { get; set; } = new Environment();
        public List<Character> Players { get; set; } = new List<Character>();
    }
}
