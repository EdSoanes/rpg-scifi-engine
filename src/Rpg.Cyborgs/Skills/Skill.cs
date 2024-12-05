using Newtonsoft.Json;
using Rpg.ModObjects.Activities;
using Rpg.ModObjects.Mods.Mods;

namespace Rpg.Cyborgs.Skills
{
    public abstract class Skill : ActionTemplate<Actor>
    {
        protected string RatingProp => $"{GetType().Name}_Rating";

        [JsonConstructor] protected Skill() 
            : base() { }

        public Skill(Actor owner)
            : base(owner) 
        {
            Classification = "Skill";
        }

        [JsonIgnore]
        public int Rating
        {
            get => Graph?.CalculatePropValue(OwnerId!, RatingProp)?.Roll() ?? 0;
            set => Graph?.AddMods(new Initial(OwnerId!, RatingProp, value));
        }

        [JsonProperty] public bool IsIntrinsic { get; protected set; }
    }
}
