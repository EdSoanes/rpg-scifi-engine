using Newtonsoft.Json;

namespace Rpg.Cyborgs.Conditions
{
    public class Attachable : Condition<BodyPart>
    {
        [JsonConstructor] private Attachable() { }

        public Attachable(BodyPart owner)
            : base(owner) { }

        protected override bool IsOnWhen(BodyPart owner)
            => owner.BodyPartType == BodyPartType.Limb && owner.InjurySeverity == (int)InjurySeverityEnum.Severed;
    }
}
