using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts.Core.Rules
{
    public static class Stats
    {
        public static int StatBonus(int stat)
        {
            return (int)Math.Floor(((double)stat - 10) / 2);
        }
    }
}
