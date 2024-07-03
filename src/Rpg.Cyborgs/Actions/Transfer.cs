using Rpg.ModObjects;
using Rpg.ModObjects.Lifecycles;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Cyborgs.Actions
{
    public class Transfer : ModObjects.Actions.Action<RpgEntity>
    {
        public override bool IsEnabled<TOwner, TInitiator>(TOwner owner, TInitiator initiator)
        {
            throw new NotImplementedException();
        }

        public ModSet OnCost(int actionNo, RpgEntity owner, Actor initiator, RpgContainer from, RpgContainer to)
        {
            return new ModSet(new TimeLifecycle(TimePoints.BeginningOfEncounter));
        }

        public ModSet OnAct(int actionNo, RpgEntity owner, Actor initiator, RpgContainer from, RpgContainer to)
        {
            var modSet = new ModSet(new TimeLifecycle(TimePoints.Encounter(1)));
            return modSet;
        }

        public ModSet[] OnOutcome(int actionNo, RpgEntity owner, Actor initiator, RpgContainer from, RpgContainer to)
        {
            from.Remove(owner);
            to.Add(owner);

            return Array.Empty<ModSet>();
        }

    }
}
