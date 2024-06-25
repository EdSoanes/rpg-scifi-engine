using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time;
using Rpg.ModObjects;
using Rpg.Sys.Archetypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rpg.ModObjects.Meta.Attributes;

namespace Rpg.Sys.Actions
{
    [Action(Required = true)]
    public class Move : ModObjects.Actions.Action<Actor>
    {
        [JsonConstructor] private Move() { }

        public Move(Actor owner)
            : base(owner) { }

        public override bool IsEnabled<TOwner>(TOwner owner, RpgEntity initiator)
            => true;

        public ModSet OnCost(Actor owner, int distance)
        {
            var movementCost = CalculateMoveCost(owner, distance);
            return new ModSet(owner)
                .AddMod(new TurnMod(), owner, x => x.Actions.Action, -movementCost);
        }

        public ModSet OnAct(Actor owner, int distance)
        {
            return new ModSet(owner)
                .AddMod(new TurnMod(), owner, $"{nameof(Move)}.{nameof(OnAct)}", distance);
        }

        public ModSet[] OnOutcome(Actor owner, int distance)
        {
            var moveSet = new ModSet(owner)
                .AddMod(new TurnMod(), owner, x => x.Movement.Speed.Current, distance);

            var res = new List<ModSet>
            {
                moveSet,
                owner.GetState(nameof(Move))!
            };

            return res.ToArray();
        }

        private int CalculateMoveCost(Actor actor, int distance)
        {
            var movementCost = Convert.ToInt32(Math.Ceiling((double)distance / actor.Movement.Speed.Max));
            return movementCost;
        }
    }
}
