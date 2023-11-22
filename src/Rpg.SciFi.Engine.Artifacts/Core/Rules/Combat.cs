using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts.Core.Rules
{
    public static class Combat
    {
        public static Modifier[] CalculateToHit(Environment environment, Character character, Artifact weapon, Artifact target, int range)
        {
            var mods = new List<Modifier>();
            return mods.ToArray();
        }
    }
}
