using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Archetypes;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Modifiers;

namespace Rpg.SciFi.Engine.Artifacts.Actions
{
    public abstract class BaseAction : ModdableObject
    {
        private readonly ActionCost _actionCost;

        [JsonProperty] protected int? Resolution { get; set; }
        
        [JsonConstructor] private BaseAction() { }

        protected BaseAction(EntityGraph entityGraph, string name, ActionCost actionCost)
        {
            Initialize(entityGraph);

            Name = name;

            _actionCost = actionCost;

            Graph!.Mods!.Add(Setup());
        }

        protected BaseAction(EntityGraph entityGraph, string name, int actionCost, int exertionCost, int focusCost)
            : this(entityGraph, name, new ActionCost(actionCost, exertionCost, focusCost))
        {
        }

        [Moddable] public int BaseActionCost { get => Resolve(); }
        [Moddable] public int BaseExertionCost { get => Resolve(); }
        [Moddable] public int BaseFocusCost { get => Resolve(); }

        [Moddable] public int ActionCost { get => Resolve(); }
        [Moddable] public int ExertionCost { get => Resolve(); }
        [Moddable] public int FocusCost { get => Resolve(); }

        public virtual bool IsResolved { get => Resolution != null; }
        protected abstract void OnAct(Actor actor, int diceRoll = 0);
        protected abstract BaseAction? NextAction();


        public override Modifier[] Setup()
        {
            return new[]
            {
                BaseModifier.Create(this, _actionCost.Action, x => BaseActionCost),
                BaseModifier.Create(this, _actionCost.Exertion, x => BaseExertionCost),
                BaseModifier.Create(this, _actionCost.Focus, x => BaseFocusCost),

                BaseModifier.Create(this, x => BaseActionCost, x => ActionCost),
                BaseModifier.Create(this, x => BaseExertionCost, x => ExertionCost),
                BaseModifier.Create(this, x => BaseFocusCost, x => FocusCost)
            };
        }

        public BaseAction? Act(Actor actor, int diceRoll = 0)
        {
            if (!IsResolved)
            {
                Resolution = diceRoll;

                var actionCost = ActionCost;
                if (actionCost != 0)
                    Graph!.Mods!.Add(CostModifier.Create(actionCost, actor, x => x.Turns.Action, () => Rules.Minus));

                var exertionCost = ExertionCost;
                if (exertionCost != 0)
                    Graph!.Mods!.Add(CostModifier.Create(exertionCost, actor, x => x.Turns.Exertion, () => Rules.Minus));

                var focusCost = FocusCost;
                if (focusCost != 0)
                    Graph!.Mods!.Add(CostModifier.Create(focusCost, actor, x => x.Turns.Focus, () => Rules.Minus));

                OnAct(actor, diceRoll);

                Graph!.Mods!.Remove(Id);
            }

            return NextAction();
        }
    }
}
