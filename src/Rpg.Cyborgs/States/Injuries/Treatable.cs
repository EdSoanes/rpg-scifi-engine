using Newtonsoft.Json;
using Rpg.ModObjects.States;

namespace Rpg.Cyborgs.States
{
    public class Treatable : State<BodyPart>
    {
        [JsonConstructor] private Treatable() { }

        public Treatable(BodyPart owner)
            : base(owner) { }

        protected override bool IsOnWhen(BodyPart owner)
            => owner.InjurySeverity == (int)InjurySeverityEnum.Unusable;
    }
}
