using Newtonsoft.Json;

namespace Rpg.Cyborgs.Conditions
{
    public class Unconscious : Condition<Actor>
    {
        [JsonConstructor] private Unconscious() { }

        public Unconscious(Actor owner)
            : base(owner) { }

        protected override bool IsOnWhen(Actor owner)
            => owner.Head.InjurySeverity == (int)InjurySeverityEnum.Mangled;
    }
}
