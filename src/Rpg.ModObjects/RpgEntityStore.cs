using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rpg.ModObjects.Stores;

namespace Rpg.ModObjects
{
    public class RpgEntityStore : ModBaseStore<string, List<RpgEntity>>
    {
        public RpgEntityStore(string entityId)
            : base(entityId) { }
    }
}
