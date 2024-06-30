using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.Cyborgs.Skills
{
    public abstract class Skill : ModObjects.Actions.Action<Actor>
    {
        protected string TaskCheckRollProp => $"{GetType().Name}.TaskCheckRoll";
        protected string RatingProp => $"{GetType().Name}.Rating";

        protected RpgGraph Graph { get; set; }


        [JsonIgnore]
        public Dice TaskCheckRoll
        {
            get
            {
                var actor = Graph.GetEntity<Actor>(OwnerId)!;
                var diceRoll = Graph.CalculatePropValue(actor, TaskCheckRollProp) ?? Dice.Zero;

                return diceRoll;
            }
        }

        [JsonIgnore]
        public int Rating
        {
            get
            {
                var actor = Graph.GetEntity<Actor>(OwnerId)!;
                var rating = Graph.CalculatePropValue(new PropRef(OwnerId!, RatingProp)) ?? Dice.Zero;

                return rating.Roll();
            }
            set
            {
                var actor = Graph.GetEntity<Actor>(OwnerId)!;
                actor.InitMod(RatingProp, value);
            }
        }

        [JsonProperty] public bool IsIntrinsic { get; protected set; }

        [JsonConstructor] protected Skill() { }

        public Skill(Actor owner)
            : base(owner) { }

        protected void ModCheck(ModSet modSet, Dice dice)
        {
            var actor = Graph.GetEntity<Actor>(OwnerId)!;
            modSet.AddMod(new TurnMod(), actor, TaskCheckRollProp, dice);
        }

        protected void ModCheck<TSourceValue>(ModSet modSet, Expression<Func<Actor, TSourceValue>> sourceExpr)
        {
            var actor = Graph.GetEntity<Actor>(OwnerId)!;
            modSet.AddMod(new TurnMod(), actor, TaskCheckRollProp, sourceExpr);
        }

        public void OnBeforeTime(RpgGraph graph, RpgEntity entity)
        {
            Graph = graph;
        }
    }
}
