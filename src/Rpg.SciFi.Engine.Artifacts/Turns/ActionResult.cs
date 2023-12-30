using Rpg.SciFi.Engine.Artifacts.MetaData;
using Rpg.SciFi.Engine.Artifacts.Modifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts.Turns
{
    public class ActionResult
    {
        private List<Modifier> _modifiers = new List<Modifier>();

        public void Add(params Modifier[] modifiers) => _modifiers.AddRange(modifiers);

        public bool ShouldResolve(int target = 0, int result = 0) => result >= target;

        public void Resolve(IContext context)
        {

        }
    }
}
