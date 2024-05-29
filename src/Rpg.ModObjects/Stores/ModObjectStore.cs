using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Stores
{
    public class ModObjectStore : ModBaseStore<Guid, RpgObject>
    {
        public override void OnBeginEncounter() { }
        public override void OnEndEncounter() { }
        public override void OnTurnChanged(int turn) { }
    }
}
