using Newtonsoft.Json;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Mods;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Values;

namespace Rpg.Cyborgs.Skills
{
    public abstract class Skill : ModObjects.Actions.Action<Actor>
    {
        protected string RatingProp => $"{GetType().Name}_Rating";

        [JsonIgnore]
        public int Rating
        {
            get
            {
                var actor = Graph.GetObject<Actor>(OwnerId)!;
                var rating = Graph.CalculatePropValue(new PropRef(OwnerId!, RatingProp)) ?? Dice.Zero;

                return rating.Roll();
            }
            set
            {
                var actor = Graph.GetObject<Actor>(OwnerId)!;
                actor.AddMods(new Initial(actor.Id, RatingProp, value));
            }
        }

        [JsonProperty] public bool IsIntrinsic { get; protected set; }

        [JsonConstructor] protected Skill() { }

        public Skill(Actor owner)
            : base(owner) { }
    }
}
