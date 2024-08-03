using Newtonsoft.Json;
using Rpg.ModObjects.States;

namespace Rpg.Cyborgs.States
{
    public class Dead : State<BodyPart>
    {
        [JsonConstructor] private Dead() { }

        public Dead(BodyPart owner)
            : base(owner) { }

        protected override bool IsOnWhen(BodyPart owner)
            => (owner.BodyPartType == BodyPartType.Torso || owner.BodyPartType == BodyPartType.Head) && owner.InjurySeverity >= (int)InjurySeverityEnum.Severed;
    }
}
