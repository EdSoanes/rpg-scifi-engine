using Newtonsoft.Json;
using Rpg.ModObjects.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Actions
{
    public class ActionState : States.State<RpgEntity>
    {
        [JsonConstructor] protected ActionState() { }

        public ActionState(RpgEntity owner, string name)
            : base(owner)
        {
            Name = name;
        }

        protected override bool IsOnWhen(RpgEntity owner)
            => false;

        protected override void WhenOn(RpgEntity owner) { }
    }
}
