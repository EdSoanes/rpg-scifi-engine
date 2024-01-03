using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Archetypes;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.Modifiers;
using System.Linq.Expressions;

namespace Rpg.SciFi.Engine.Artifacts.Actions
{
    public class TurnAction : BaseAction
    {
        [JsonProperty] private int? ResolutionTarget { get; set; }

        [JsonProperty] public List<Modifier> Success { get; private set; } = new List<Modifier>();
        [JsonProperty] public List<Modifier> Failure { get; private set; } = new List<Modifier>();

        [Moddable] public Dice DiceRoll { get => Evaluate(); }
        [Moddable] public Dice DiceRollTarget { get => Evaluate(); }

        private TurnAction? SuccessAction { get; set; }
        private TurnAction? FailureAction { get; set; }

        public TurnAction(ModStore modStore, PropEvaluator evaluator, string name, int actionCost, int exertionCost, int focusCost)
            : base(modStore, evaluator, name, actionCost, exertionCost, focusCost)
        {
        }

        protected override void OnAct(Actor actor, int diceRoll = 0)
        {
            ResolutionTarget = DiceRollTarget.Roll();
            var modifiers = Resolution >= ResolutionTarget
                ? Success
                : Failure;

            ModStore!.Add(modifiers.ToArray());
        }

        protected override BaseAction? NextAction()
        {
            if (IsResolved)
            {
                var res = Resolution >= ResolutionTarget
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
