using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.ModObjects.Meta.Props;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Values;
using Rpg.Sys.Components;

namespace Rpg.Sys.Archetypes
{
    public abstract class Actor : Artifact
    {
        [JsonProperty]
        [Component(Group = "Stats")]
        public StatPoints Stats { get; private set; }

        [JsonProperty]
        [Component(Group = "Movement")]
        public Movement Movement { get; private set; }

        [JsonProperty]
        [Component(Group = "Actions")] 
        public ActionPoints Actions { get; private set; }
        
        [JsonConstructor] protected Actor() { }

        public Actor(ActorTemplate template)
            : base(template)
        {
            Movement = new Movement(Id, nameof(Movement), template.Movement);
            Actions = new ActionPoints(Id, nameof(Actions), template.Actions);
            Stats = new StatPoints(Id, nameof(Stats), template.Stats);
        }

        public override void OnBeforeTime(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnBeforeTime(graph, this);
            Movement.OnBeforeTime(graph, this);
            Actions.OnBeforeTime(graph, this);
            Stats.OnBeforeTime(graph, this);
        }

        protected override void OnLifecycleStarting()
        {
            this.BaseMod(x => x.Actions.Exertion, x => x.Stats.Strength.Bonus);
            this.BaseMod(x => x.Actions.Action, x => x.Stats.Dexterity.Bonus);
            this.BaseMod(x => x.Actions.Focus, x => x.Stats.Intelligence.Bonus);

            this.BaseMod(x => x.Movement.Speed.Max, x => x.Stats.Dexterity.Bonus);
            this.BaseMod(x => x.Movement.Speed.Max, x => x.Presence.Weight, () => DiceCalculations.WeightSpeedBonus);
            this.BaseMod(x => x.Movement.Speed.Max, x => x.Stats.Strength.Bonus);
        }

        //public virtual ActionBase Move(int distance)
        //{
        //    var actionCost = Convert.ToInt32(Math.Ceiling((double)distance / this.Movement.Acceleration));

        //    return new TurnAction(nameof(Move), actionCost, 1, 0)
        //        .OnSuccess(TurnModifier.Create(this, distance, x => x.Movement.Speed));
        //}

        //public virtual ActionBase Evade(int cost)
        //{
        //    return new TurnAction(nameof(Evade), cost, cost, 1)
        //        .OnSuccess(TurnModifier.Create(this, cost, x => x.Defenses.Evasion));
        //}

        //public virtual ActionBase Conceal(int bonus)
        //{
        //    return new TurnAction(nameof(Conceal), 1, 1, 0)
        //        .OnSuccess(TurnModifier.Create(this, bonus, x => x.Defenses.Concealment));
        //}

        //public virtual ActionBase Transfer(Container? source, Container? target, Artifact artifact)
        //{
        //    return new TransferItemAction(nameof(Transfer), source, target, artifact);
        //}
    }
}
