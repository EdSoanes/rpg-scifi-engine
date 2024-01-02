using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.Modifiers;
using System.Linq.Expressions;

namespace Rpg.SciFi.Engine.Artifacts.MetaData
{
    public class TurnAction : ModdableObject
    {
        private readonly int _baseAction;
        private readonly int _baseExertion;
        private readonly int _baseFocus;

        [JsonProperty] private int? Resolution { get; set; }
        [JsonProperty] private int? TargetResolution { get; set; }

        [JsonConstructor] private TurnAction() { }

        public TurnAction(ModStore modStore, PropEvaluator evaluator, string name, int actionCost, int exertionCost, int focusCost)
        {
            Initialize(modStore, evaluator);

            Name = name;

            _baseAction = actionCost;
            _baseExertion = exertionCost;
            _baseFocus = focusCost;

            ModStore.Add(Setup());
        }

        [Moddable] public int BaseActionCost { get => Resolve(); }
        [Moddable] public int BaseExertionCost { get => Resolve(); }
        [Moddable] public int BaseFocusCost { get => Resolve(); }

        [Moddable] public int ActionCost { get => Resolve(); }
        [Moddable] public int ExertionCost { get => Resolve(); }
        [Moddable] public int FocusCost { get => Resolve(); }

        [JsonProperty] public List<Modifier> Success { get; private set; } = new List<Modifier>();
        [JsonProperty] public List<Modifier> Failure { get; private set; } = new List<Modifier>();

        [Moddable] public Dice DiceRoll { get => Evaluate(); }
        [Moddable] public Dice DiceRollTarget { get => Evaluate(); }

        public bool IsResolved { get => Resolution != null; }

        private TurnAction? SuccessAction { get; set; }
        private TurnAction? FailureAction { get; set; }

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

        public virtual TurnAction? Act(Actor actor, int diceRoll = 0)
        {
            if (!IsResolved)
            {
                Resolution = diceRoll;
                TargetResolution = DiceRollTarget.Roll();

                var actionCost = ActionCost;
                if (actionCost != 0)
                    ModStore!.Add(CostModifier.Create(actionCost, actor, x => x.Turns.Action, () => Rules.Minus));

                var exertionCost = ExertionCost;
                if (exertionCost != 0)
                    ModStore!.Add(CostModifier.Create(exertionCost, actor, x => x.Turns.Exertion, () => Rules.Minus));

                var focusCost = FocusCost;
                if (focusCost != 0)
                    ModStore!.Add(CostModifier.Create(focusCost, actor, x => x.Turns.Focus, () => Rules.Minus));

                var modifiers = Resolution >= TargetResolution
                    ? Success
                    : Failure;

                ModStore!.Add(modifiers.ToArray());
                ModStore.Remove(Id);
            }

            return NextAction();
        }

        public TurnAction? NextAction()
        {
            if (IsResolved)
            {
                var res = Resolution >= TargetResolution
                    ? SuccessAction
                    : FailureAction;

                return res;
            }

            return null;
        }

        public TurnAction OnSuccess(Modifier mod)
        {
            if (!IsResolved && !Success.Any(x => x.Id == mod.Id))
                Success.Add(mod);

            return this;
        }

        public TurnAction OnFailure(Modifier mod)
        {
            if (!IsResolved && !Failure.Any(x => x.Id == mod.Id))
                Failure.Add(mod);

            return this;
        }

        public TurnAction OnDiceRoll(Dice dice)
        {
            ModStore!.Add(BaseModifier.Create(this, dice, x => x.DiceRoll));
            return this;
        }

        public TurnAction OnDiceRoll(string name, Dice dice, Expression<Func<Func<Dice, Dice>>>? diceCalc = null)
        {
            ModStore!.Add(BaseModifier.Create(this, name, dice, x => x.DiceRoll, diceCalc));
            return this;
        }

        public TurnAction OnDiceRoll<T, TR>(T source, Expression<Func<T, TR>> sExpr, Expression<Func<Func<Dice, Dice>>>? diceCalc = null)
            where T : ModdableObject
        {
            ModStore!.Add(BaseModifier.Create(source, sExpr, this, x => x.DiceRoll, diceCalc));
            return this;
        }

        public TurnAction OnDiceRollTarget(Dice dice, Expression<Func<Func<Dice, Dice>>>? diceCalc = null)
        {
            ModStore!.Add(BaseModifier.Create(this, dice, x => x.DiceRollTarget, diceCalc));
            return this;
        }

        public TurnAction OnDiceRollTarget<T, TR>(T source, Expression<Func<T, TR>> sExpr, Expression<Func<Func<Dice, Dice>>>? diceCalc = null)
            where T : ModdableObject
        {
            ModStore!.Add(BaseModifier.Create(source, sExpr, this, x => x.DiceRollTarget, diceCalc));
            return this;
        }
    }
}
