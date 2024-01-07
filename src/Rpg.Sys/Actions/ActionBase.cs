using Newtonsoft.Json;
using Rpg.Sys.Archetypes;
using Rpg.Sys.Modifiers;

namespace Rpg.Sys.Actions
{
    public abstract class ActionBase : ModdableObject
    {
        [JsonProperty] public ActionCost Cost { get; private set; }
        [JsonProperty] protected int? Resolution { get; set; }
        public virtual bool IsResolved { get => Resolution != null; }

        [JsonConstructor] protected ActionBase() { }

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
                //Only apply costs if within an encounter, otherwise irrelevant
                if (graph.Turn > 0)
                { 
                    var costMods = OnResolutionCost(actor);
                    graph.Mods.Add(costMods);
                }

                Resolution = diceRoll;
                OnResolve(actor, graph);
            }

            return NextAction();
        }

        protected abstract void OnResolve(Actor actor, Graph graph);
        protected virtual ActionBase? NextAction() => null;
    }
}
