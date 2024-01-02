using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Modifiers;

namespace Rpg.SciFi.Engine.Artifacts.MetaData
{
    public abstract class BaseAction : ModdableObject
    {
        private readonly int _baseAction;
        private readonly int _baseExertion;
        private readonly int _baseFocus;

        [JsonProperty] protected int? Resolution { get; set; }
        
        [JsonConstructor] private BaseAction() { }

        protected BaseAction(ModStore modStore, PropEvaluator evaluator, string name, int actionCost, int exertionCost, int focusCost)
        {
            Initialize(modStore, evaluator);

            Name = name;

            _baseAction = actionCost;
            _baseExertion = exertionCost;
            _baseFocus = focusCost;

            ModStore!.Add(Setup());
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
                BaseModifier.Create(this, _baseAction, x => BaseActionCost),
                BaseModifier.Create(this, _baseExertion, x => BaseExertionCost),
                BaseModifier.Create(this, _baseFocus, x => BaseFocusCost),

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
                    ModStore!.Add(CostModifier.Create(actionCost, actor, x => x.Turns.Action, () => Rules.Minus));

                var exertionCost = ExertionCost;
                if (exertionCost != 0)
                    ModStore!.Add(CostModifier.Create(exertionCost, actor, x => x.Turns.Exertion, () => Rules.Minus));

                var focusCost = FocusCost;
                if (focusCost != 0)
                    ModStore!.Add(CostModifier.Create(focusCost, actor, x => x.Turns.Focus, () => Rules.Minus));

                OnAct(actor, diceRoll);

                ModStore!.Remove(Id);
            }

            return NextAction();
        }
    }
}
