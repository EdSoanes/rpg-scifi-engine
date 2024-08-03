using Newtonsoft.Json;

namespace Rpg.Cyborgs.Conditions
{
    public class Unusable : Condition<BodyPart>
    {
        [JsonConstructor] private Unusable() { }

        public Unusable(BodyPart owner)
            : base(owner) { }

        protected override bool IsOnWhen(BodyPart owner)
            => owner.InjurySeverity >= (int)InjurySeverityEnum.Unusable && owner.BodyPartType == BodyPartType.Limb;
    }
}
