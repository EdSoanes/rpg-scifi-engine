using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Mods.Templates
{
    public class ExpireOnZeroMod : ExpiresOnMod
    {
        public ExpireOnZeroMod()
            : base(0)
        { }
    }
}
