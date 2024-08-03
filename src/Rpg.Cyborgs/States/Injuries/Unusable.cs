using Newtonsoft.Json;
using Rpg.ModObjects.States;

namespace Rpg.Cyborgs.States
{
    public class Unusable : State<BodyPart>
    {
        [JsonConstructor] private Unusable() { }

        public Unusable(BodyPart owner)
            : base(owner) { }

        protected override bool IsOnWhen(BodyPart owner)
            => owner.InjurySeverity >= (int)InjurySeverityEnum.Unusable && owner.BodyPartType == BodyPartType.Limb;
    }
}
