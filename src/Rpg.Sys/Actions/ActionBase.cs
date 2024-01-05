using Newtonsoft.Json;
using Rpg.Sys.Archetypes;
using Rpg.Sys.Modifiers;

namespace Rpg.Sys.Actions
{
    public abstract class ActionBase : ModdableObject
    {
        [JsonProperty] protected ActionCost Cost { get; set; }
        [JsonProperty] protected int? Resolution { get; set; }
        public virtual bool IsResolved { get => Resolution != null; }

        protected ActionBase(string name, ActionCost actionCost) 
        {
            Name = name;
            Cost = actionCost; 
        }

        protected virtual Modifier[] OnResolutionCost(Actor actor)
        {
            var mods = new List<Modifier>();

            if (Cost.Action != 0)
                mods.Add(CostModifier.Create(Cost.Action, actor, x => x.Actions.Action.Current, () => DiceCalculations.Minus));

            if (Cost.Exertion != 0)
                mods.Add(CostModifier.Create(Cost.Exertion, actor, x => x.Actions.Exertion.Current, () => DiceCalculations.Minus));

            if (Cost.Focus != 0)
                mods.Add(CostModifier.Create(Cost.Focus, actor, x => x.Actions.Focus.Current, () => DiceCalculations.Minus));

            return mods.ToArray();
        }

        public ActionBase? Resolve(Actor actor, Graph graph, int diceRoll = 0)
        {
            if (!IsResolved)
            {
                var mods = OnResolutionCost(actor);
                graph.Mods.Add(mods);

                Resolution = diceRoll;
                OnResolve(actor, graph);
            }

            return NextAction();
        }

        public ActionBase? Act(Actor actor, int diceRoll = 0)
        {
            if (!IsResolved)
            {
                Resolution = diceRoll;
                Act(actor);
            }

            return NextAction();
        }

        protected abstract void OnResolve(Actor actor, Graph graph);
        protected virtual ActionBase? NextAction() => null;
    }
}
