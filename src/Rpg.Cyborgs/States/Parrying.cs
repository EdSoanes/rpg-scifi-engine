using Newtonsoft.Json;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Cyborgs.States
{
    public class Parrying : State<Actor>
    {
        [JsonConstructor] private Parrying() { }

        public Parrying(Actor owner)
            : base(owner) { }
    }
}
