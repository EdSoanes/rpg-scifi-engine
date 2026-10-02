using Newtonsoft.Json;
using Rpg.Experimental;
using Rpg.Experimental.Graph;

namespace Rpg.Cyborgs.Skills
{
    public abstract class Skill : RpgAction<Actor>
    {
        /// <summary>
        /// The skill rating is a virtual property on the actor that owns the skill, so it can be modded like
        /// any other property
        /// </summary>
        public string RatingProp { get => $"Rating/{GetType().Name}"; }

        [JsonProperty] public bool IsIntrinsic { get; protected set; }
        [JsonProperty] public int InitialRating { get; protected set; }

        [JsonConstructor] protected Skill()
            : base() { }

        public Skill(Actor owner)
            : base(owner)
        {
            Classification = "Skill";
        }

        public int Rating(RpgGraph graph)
            => graph.GetPropertyValue<Dice>(OwnerId!, RatingProp).Roll();

        public override void OnCreating(RpgGraph graph, RpgObject? owner)
        {
            base.OnCreating(graph, owner);

            var actor = graph.GetObject(OwnerId);
            if (actor != null)
                graph.CreateVirtualProperty(actor, RatingProp, InitialRating);
        }
    }
}
