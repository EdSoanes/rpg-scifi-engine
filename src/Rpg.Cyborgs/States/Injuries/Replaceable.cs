using Newtonsoft.Json;
using Rpg.ModObjects.States;

namespace Rpg.Cyborgs.States
{
    public class Replaceable : State<BodyPart>
    {
        [JsonConstructor] private Replaceable() { }

        public Replaceable(BodyPart owner)
            : base(owner) { }

        protected override bool IsOnWhen(BodyPart owner)
            => owner.BodyPartType == BodyPartType.Limb && owner.InjurySeverity >= (int)InjurySeverityEnum.Mangled;
    }
}
