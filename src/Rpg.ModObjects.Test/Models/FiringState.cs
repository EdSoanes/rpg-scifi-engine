using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Tests.Models
{
    public class FiringState<T> : ModState
        where T : TestGun
    {
        public FiringState(string name)
            => Name = name;

        protected override bool ShouldActivate()
            => Graph!.GetEntity<T>(EntityId)?.GetPropValue(InstanceName) != null;
    }
}
