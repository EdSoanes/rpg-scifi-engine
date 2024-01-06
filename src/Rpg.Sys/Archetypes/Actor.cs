using Newtonsoft.Json;
using Rpg.Sys.Actions;
using Rpg.Sys.Components;
using Rpg.Sys.Modifiers;

namespace Rpg.Sys.Archetypes
{
    public class Actor : Artifact
    {
        [JsonProperty] public Movement Movement { get; private set; }
        [JsonProperty] public ActionPoints Actions { get; private set; }
        [JsonProperty] public StatPoints Stats { get; private set; }

        [JsonConstructor] protected Actor() { }

        public Actor(ActorTemplate template)
            : base(template)
        {
            Movement = new Movement(template.Movement);
            Actions = new ActionPoints(template.Actions);
            Stats = new StatPoints(template.Stats);
        }

        public override Modifier[] SetupModdableProperties(Graph graph)
        {
            var mods = new List<Modifier>(base.SetupModdableProperties(graph))
            {
                BaseModifier.Create(this, x => x.Stats.Strength.Bonus, x => x.Actions.Exertion.Max),
                BaseModifier.Create(this, x => x.Stats.Dexterity.Bonus, x => x.Actions.Action.Max),
                BaseModifier.Create(this, x => x.Stats.Intelligence.Bonus, x => x.Actions.Focus.Max)
            };

            return mods.ToArray();
        }

        public virtual ActionBase? ActivateState(Artifact artifact, string stateName)
        {
            var state = artifact.States.FirstOrDefault(x => x.Name == stateName);
            return state != null
                ? new StateAction(artifact, state, true)
                : null;
        }

        public virtual ActionBase? DeactivateState(Artifact artifact, string stateName)
        {
            var state = artifact.States.FirstOrDefault(x => x.Name == stateName);
            return state != null
                ? new StateAction(artifact, state, false)
                : null;
        }

        public virtual ActionBase Move(int distance)
        {
            var actionCost = Convert.ToInt32(Math.Ceiling((double)distance / this.Movement.Acceleration));

            return new TurnAction(nameof(Move), actionCost, 1, 0)
                .OnSuccess(TurnModifier.Create(this, distance, x => x.Movement.Speed));
        }

        public virtual ActionBase Evade(int cost)
        {
            return new TurnAction(nameof(Evade), cost, cost, 1)
                .OnSuccess(TurnModifier.Create(this, cost, x => x.Defenses.Evasion));
        }

        public virtual ActionBase Conceal(int bonus)
        {
            return new TurnAction(nameof(Conceal), 1, 1, 0)
                .OnSuccess(TurnModifier.Create(this, bonus, x => x.Defenses.Concealment));
        }

        public virtual ActionBase Transfer(Container source, Container target, Artifact artifact)
        {
            return new TransferItemAction(nameof(Transfer), source, target, artifact);
        }
    }
}
