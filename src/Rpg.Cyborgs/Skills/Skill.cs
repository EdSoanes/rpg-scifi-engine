using Newtonsoft.Json;
using Rpg.ModObjects.Activities;
using Rpg.ModObjects.Mods.Mods;

namespace Rpg.Cyborgs.Skills
{
    public abstract class Skill : ActionTemplate<Actor>
    {
        public string RatingProp { get => $"{GetType().Name}_Rating"; }
        public int Rating { get => Graph?.GetObject(OwnerId)?.Value(RatingProp)?.Roll() ?? 0; }

        [JsonProperty] public bool IsIntrinsic { get; protected set; }

        [JsonConstructor] protected Skill() 
            : base() { }

        public Skill(Actor owner)
            : base(owner) 
        {
            Classification = "Skill";
        }
    }
}
