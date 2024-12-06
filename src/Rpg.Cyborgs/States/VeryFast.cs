using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Mods;
using Rpg.ModObjects.Mods.ModSets;
using Rpg.ModObjects.States;
using Newtonsoft.Json;

namespace Rpg.Cyborgs.States
{
    public class VeryFast : State<Actor>
    {
        [JsonConstructor] private VeryFast() { }

        public VeryFast(Actor owner)
            : base(owner) { }

        protected override bool IsOnWhen(Actor owner)
            => owner.Reactions.Value > 10;

        protected override void OnFillStateSet(StateModSet modSet, Actor owner)
        {
            base.OnFillStateSet(modSet, owner);
            modSet.Add(new Permanent(modSet.Id), owner, x => x.ActionPoints, 1);
        }
    }
}
