using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Tests.Models;
using Rpg.ModObjects.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Tests.Actions
{
    public class TestAction : ModObjects.Actions.Action<ModdableEntity>
    {
        public TestAction(ModdableEntity owner)
            : base(owner) { }

        public override bool IsEnabled<TOwner>(TOwner owner, RpgEntity initiator)
            => true;

        public ModSet OnCost(ModdableEntity owner, TestHuman initiator)
        {
            return new ModSet(owner)
                .AddMod(new TurnMod(), initiator, x => x.PhysicalActionPoints.Current, -1);
        }

        public ModSet OnAct(ModdableEntity owner, TestHuman initiator, int target)
        {
            return new ModSet(owner);
        }

        public ModSet[] OnOutcome(ModdableEntity owner, TestHuman initiator, int diceRoll)
        {
            var res = new List<ModSet>
            {
                owner.GetState(nameof(TestAction))!
            };

            return res.ToArray();
        }


    }
}
