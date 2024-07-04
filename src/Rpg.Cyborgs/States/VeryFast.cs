using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Templates;
using Rpg.ModObjects.States;

namespace Rpg.Cyborgs.States
{
    public class VeryFast : State<Actor>
    {
        public VeryFast(Actor owner)
            : base(owner) { }

        protected override bool IsOnWhen(Actor owner)
            => owner.Reactions > 10;

        protected override void OnFillStateSet(ModSet modSet, Actor owner)
        {
            base.OnFillStateSet(modSet, owner);
            modSet.Add(new PermanentMod(), owner, x => x.Actions, 1);
        }
    }
}
