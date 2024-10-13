using Newtonsoft.Json;

namespace Rpg.Cyborgs.Conditions
{
    public class Dying : Condition<Actor>
    {
        [JsonConstructor] private Dying() { }

        public Dying(Actor owner)
            : base(owner) { }

        protected override bool IsOnWhen(Actor owner)
            => !owner.IsStateOn(nameof(Dead)) && (
                owner.Head.InjurySeverity >= (int)InjurySeverityEnum.Busted && owner.Head.InjurySeverity < (int)InjurySeverityEnum.Severed
                || owner.Torso.InjurySeverity == (int)InjurySeverityEnum.Mangled
                || owner.LeftArm.InjurySeverity > (int)InjurySeverityEnum.Mangled
                || owner.RightArm.InjurySeverity > (int)InjurySeverityEnum.Mangled
                || owner.LeftLeg.InjurySeverity > (int)InjurySeverityEnum.Mangled
                || owner.RightLeg.InjurySeverity > (int)InjurySeverityEnum.Mangled);
    }
}
