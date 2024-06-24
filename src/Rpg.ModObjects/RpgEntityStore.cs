using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects
{
    internal class RpgEntityStore : RpgBaseStore<string, List<RpgEntity>>
    {
        public RpgEntityStore(string entityId)
            : base(entityId) { }
    }
}
