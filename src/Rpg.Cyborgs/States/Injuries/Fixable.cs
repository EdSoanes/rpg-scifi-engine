using Newtonsoft.Json;
using Rpg.ModObjects.States;

namespace Rpg.Cyborgs.States
{
    public class Fixable : State<BodyPart>
    {
        [JsonConstructor] private Fixable() { }

        public Fixable(BodyPart owner)
            : base(owner) { }

        protected override bool IsOnWhen(BodyPart owner)
            => owner.BodyPartType == BodyPartType.Limb && owner.InjurySeverity == (int)InjurySeverityEnum.Busted;
    }
}
