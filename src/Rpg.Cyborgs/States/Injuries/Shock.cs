using Newtonsoft.Json;
using Rpg.ModObjects.States;

namespace Rpg.Cyborgs.States
{
    public class Shock : State<BodyPart>
    {
        [JsonConstructor] private Shock() { }

        public Shock(BodyPart owner)
            : base(owner) { }

        protected override bool IsOnWhen(BodyPart owner)
            => (owner.BodyPartType == BodyPartType.Torso && owner.InjurySeverity >= (int)InjurySeverityEnum.Unusable && owner.InjurySeverity < (int)InjurySeverityEnum.Severed)
                || (owner.BodyPartType == BodyPartType.Head && owner.InjurySeverity >= (int)InjurySeverityEnum.Unusable && owner.InjurySeverity < (int)InjurySeverityEnum.Mangled)
                || (owner.BodyPartType == BodyPartType.Limb && owner.InjurySeverity >= (int)InjurySeverityEnum.Busted);
    }
}
