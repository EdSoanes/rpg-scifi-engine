using Newtonsoft.Json;
using Rpg.ModObjects.States;

namespace Rpg.Cyborgs.States
{
    public class Repairable : State<BodyPart>
    {
        [JsonConstructor] private Repairable() { }

        public Repairable(BodyPart owner)
            : base(owner) { }

        protected override bool IsOnWhen(BodyPart owner)
            => owner.BodyPartType == BodyPartType.Limb && owner.InjurySeverity == (int)InjurySeverityEnum.Mangled;
    }
}
