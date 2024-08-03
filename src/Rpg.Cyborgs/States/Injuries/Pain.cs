using Newtonsoft.Json;
using Rpg.ModObjects.States;

namespace Rpg.Cyborgs.States
{
    public class Pain : State<BodyPart>
    {
        [JsonConstructor] private Pain() { }

        public Pain(BodyPart owner)
            : base(owner) { }

        protected override bool IsOnWhen(BodyPart owner)
            => owner.InjurySeverity == (int)InjurySeverityEnum.FleshWound;
    }
}
