using Rpg.ModObjects.Behaviors;
using Rpg.ModObjects.Lifecycles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Mods.Templates
{
    public class SyncedMod : ModTemplate
    {
        public string OwnerId { get; private set; }

        public SyncedMod(string ownerId, ModType modType = ModType.Standard)
        {
            OwnerId = ownerId;
            Behavior = new Add(modType);
            Lifecycle = new SyncedLifecycle(ownerId);
        }

        public override Mod Create(string name)
        {
            var mod = new Mod(OwnerId, name, this);
            return mod;
        }
    }
}
