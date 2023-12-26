using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.Modifiers;
using System.Linq.Expressions;

namespace Rpg.SciFi.Engine.Artifacts.Turns
{
    public class TurnAction : Entity
    {
        private readonly int _baseAction;
        private readonly int _baseExertion;
        private readonly int _baseFocus;

        [JsonProperty] private int? Resolution { get; set; }
        [JsonProperty] private int? TargetResolution { get; set; }

        [JsonConstructor] private TurnAction() { }

        public TurnAction(string name, int actionCost, int exertionCost, int focusCost)
        {
            Name = name;

            _baseAction = actionCost;
            _baseExertion = exertionCost;
            _baseFocus = focusCost;
        }

        [JsonProperty] public List<Modifier> Success { get; private set; } = new List<Modifier>();
        [JsonProperty] public List<Modifier> Failure { get; private set; } = new List<Modifier>();

        [Moddable] public int BaseAction { get => this.Resolve(nameof(BaseAction)); }
        [Moddable] public int BaseExertion { get => this.Resolve(nameof(BaseExertion)); }
        [Moddable] public int BaseFocus { get => this.Resolve(nameof(BaseFocus)); }

        [Moddable] public int Action { get => this.Resolve(nameof(Action)); }
        [Moddable] public int Exertion { get => this.Resolve(nameof(Exertion)); }
        [Moddable] public int Focus { get => this.Resolve(nameof(Focus)); }

        [Moddable] public Dice DiceRoll { get => Evaluate(nameof(DiceRoll)); }
        [Moddable] public Dice DiceRollTarget { get => Evaluate(nameof(DiceRollTarget)); }

        public bool IsResolved { get => Resolution != null; }

        private TurnAction? SuccessAction { get; set; }
        private TurnAction? FailureAction { get; set; }

        [Setup]
        public Modifier[] Setup()
        {
            return new[]
            {
                BaseModifier.Create(this, _baseAction, x => BaseAction),
                BaseModifier.Create(this, _baseExertion, x => BaseExertion),
                BaseModifier.Create(this, _baseFocus, x => BaseFocus),

                BaseModifier.Create(this, x => BaseAction, x => Action),
                BaseModifier.Create(this, x => BaseExertion, x => Exertion),
                BaseModifier.Create(this, x => BaseFocus, x => Focus)
            };
        }

        public Modifier[] Resolve(int diceRoll = 0)
        {
            if (!IsResolved)
            {
                Resolution = diceRoll;
                TargetResolution = DiceRollTarget.Roll();
            }

            var res = Resolution >= TargetResolution
                ? Success
                : Failure;

            return res.ToArray();
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

        public TurnAction OnSuccessAction(Modifier mod, string? name = null, int actionCost = 0, int exertionCost = 0, int focusCost = 0)
        {
            if (!IsResolved)
            {
                if (SuccessAction == null && !string.IsNullOrEmpty(name))
                    SuccessAction = Context.CreateTurnAction(name, actionCost, exertionCost, focusCost);

                if (SuccessAction == null)
                    throw new ArgumentException($"{nameof(OnSuccessAction)} invalid. No {nameof(SuccessAction)}", nameof(OnSuccessAction));

                SuccessAction.Success.Add(mod);
            }

            return this;
        }

        public TurnAction OnFailureAction(Modifier mod, string? name = null, int actionCost = 0, int exertionCost = 0, int focusCost = 0)
        {
            if (!IsResolved)
            {
                if (FailureAction == null && !string.IsNullOrEmpty(name))
                    FailureAction = Context.CreateTurnAction(name, actionCost, exertionCost, focusCost);

                if (FailureAction == null)
                    throw new ArgumentException($"{nameof(OnSuccessAction)} invalid. No {nameof(FailureAction)}", nameof(OnFailureAction));

                FailureAction.Failure.Add(mod);
            }

            return this;
        }

        public TurnAction OnDiceRoll(Dice dice)
        {
            Context.AddMod(BaseModifier.Create(this, dice, x => x.DiceRoll));
            return this;
        }

        public TurnAction OnDiceRoll(string name, Dice dice, Expression<Func<Func<Dice, Dice>>>? diceCalc = null)
        {
            Context.AddMod(BaseModifier.Create(this, name, dice, x => x.DiceRoll, diceCalc));
            return this;
        }

        public TurnAction OnDiceRoll<T, TR>(T source, Expression<Func<T, TR>> sExpr, Expression<Func<Func<Dice, Dice>>>? diceCalc = null)
            where T : Entity
        {
            Context.AddMod(BaseModifier.Create(source, sExpr, this, x => x.DiceRoll, diceCalc));
            return this;
        }

        public TurnAction OnDiceRollTarget(Dice dice, Expression<Func<Func<Dice, Dice>>>? diceCalc = null)
        {
            Context.AddMod(BaseModifier.Create(this, dice, x => x.DiceRollTarget, diceCalc));
            return this;
        }

        public TurnAction OnDiceRollTarget<T, TR>(T source, Expression<Func<T, TR>> sExpr, Expression<Func<Func<Dice, Dice>>>? diceCalc = null)
            where T : Entity
        {
            Context.AddMod(BaseModifier.Create(source, sExpr, this, x => x.DiceRollTarget, diceCalc));
            return this;
        }
    }
}
