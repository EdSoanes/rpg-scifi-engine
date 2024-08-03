using Newtonsoft.Json;
using Rpg.ModObjects.States;

namespace Rpg.Cyborgs.States
{
    public class Unconscious : State<Actor>
    {
        [JsonConstructor] private Unconscious() { }

        public Unconscious(Actor owner)
            : base(owner) { }

        protected override bool IsOnWhen(Actor owner)
            => owner.Head.InjurySeverity == (int)InjurySeverityEnum.Mangled;
    }
}
