using Newtonsoft.Json;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Templates;
using Rpg.ModObjects.States;

namespace Rpg.Cyborgs.States
{
    public class VeryFast : State<Actor>
    {
        [JsonConstructor] private VeryFast() { }

        public VeryFast(Actor owner)
            : base(owner) { }

        protected override bool IsOnWhen(Actor owner)
            => owner.Reactions.Value > 10;

        protected override void OnFillStateSet(ModSet modSet, Actor owner)
        {
            base.OnFillStateSet(modSet, owner);
            modSet.Add(new PermanentMod(), owner, x => x.ActionPoints, 1);
        }
    }
}
