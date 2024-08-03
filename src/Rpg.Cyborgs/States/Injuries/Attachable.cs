using Newtonsoft.Json;
using Rpg.ModObjects.States;

namespace Rpg.Cyborgs.States
{
    public class Attachable : State<BodyPart>
    {
        [JsonConstructor] private Attachable() { }

        public Attachable(BodyPart owner)
            : base(owner) { }

        protected override bool IsOnWhen(BodyPart owner)
            => owner.BodyPartType == BodyPartType.Limb && owner.InjurySeverity == (int)InjurySeverityEnum.Severed;
    }
}
