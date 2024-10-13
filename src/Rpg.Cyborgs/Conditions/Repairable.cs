using Newtonsoft.Json;

namespace Rpg.Cyborgs.Conditions
{
    public class Repairable : Condition<BodyPart>
    {
        [JsonConstructor] private Repairable() { }

        public Repairable(BodyPart owner)
            : base(owner) { }

        protected override bool IsOnWhen(BodyPart owner)
            => owner.BodyPartType == BodyPartType.Limb && owner.InjurySeverity == (int)InjurySeverityEnum.Mangled;
    }
}
