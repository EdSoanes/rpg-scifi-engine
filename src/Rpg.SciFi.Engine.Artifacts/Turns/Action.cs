using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.Modifiers;
using System.Linq.Expressions;

namespace Rpg.SciFi.Engine.Artifacts.Turns
{
    public class Action : Entity
    {
        private readonly int _baseAction;
        private readonly int _baseExertion;
        private readonly int _baseFocus;

        [JsonProperty] private int? Resolution { get; set; }
        [JsonProperty] private int? TargetResolution { get; set; }

        [JsonConstructor] private Action() { }

        public Action(string name, int actionCost, int exertionCost, int focusCost)
        {
            Name = name;

            _baseAction = actionCost;
            _baseExertion = exertionCost;
            _baseFocus = focusCost;
        }

        [Moddable] public int BaseActionCost { get => this.Resolve(nameof(BaseActionCost)); }
        [Moddable] public int BaseExertionCost { get => this.Resolve(nameof(BaseExertionCost)); }
        [Moddable] public int BaseFocusCost { get => this.Resolve(nameof(BaseFocusCost)); }

        [Moddable] public int ActionCost { get => this.Resolve(nameof(ActionCost)); }
        [Moddable] public int ExertionCost { get => this.Resolve(nameof(ExertionCost)); }
        [Moddable] public int FocusCost { get => this.Resolve(nameof(FocusCost)); }

        [JsonProperty] public List<Modifier> Success { get; private set; } = new List<Modifier>();
        [JsonProperty] public List<Modifier> Failure { get; private set; } = new List<Modifier>();

        [Moddable] public Dice DiceRoll { get => Evaluate(nameof(DiceRoll)); }
        [Moddable] public Dice DiceRollTarget { get => Evaluate(nameof(DiceRollTarget)); }

        public bool IsResolved { get => Resolution != null; }

        private Action? SuccessAction { get; set; }
        private Action? FailureAction { get; set; }

        [Setup]
        public Modifier[] Setup()
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

        public Action? NextAction()
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

        public Action OnSuccess(Modifier mod)
        {
            if (!IsResolved && !Success.Any(x => x.Id == mod.Id))
                Success.Add(mod);

            return this;
        }

        public Action OnFailure(Modifier mod)
        {
            if (!IsResolved && !Failure.Any(x => x.Id == mod.Id))
                Failure.Add(mod);

            return this;
        }

        //public Action OnSuccessAction(Modifier mod, string? name = null, int actionCost = 0, int exertionCost = 0, int focusCost = 0)
        //{
        //    if (!IsResolved)
        //    {
        //        if (SuccessAction == null && !string.IsNullOrEmpty(name))
        //            SuccessAction = EntityManager.CreateTurnAction(name, actionCost, exertionCost, focusCost);

        //        if (SuccessAction == null)
        //            throw new ArgumentException($"{nameof(OnSuccessAction)} invalid. No {nameof(SuccessAction)}", nameof(OnSuccessAction));

        //        SuccessAction.Success.Add(mod);
        //    }

        //    return this;
        //}

        //public Action OnFailureAction(Modifier mod, string? name = null, int actionCost = 0, int exertionCost = 0, int focusCost = 0)
        //{
        //    if (!IsResolved)
        //    {
        //        if (FailureAction == null && !string.IsNullOrEmpty(name))
        //            FailureAction = EntityManager.CreateTurnAction(name, actionCost, exertionCost, focusCost);

        //        if (FailureAction == null)
        //            throw new ArgumentException($"{nameof(OnFailureAction)} invalid. No {nameof(FailureAction)}", nameof(OnFailureAction));

        //        FailureAction.Failure.Add(mod);
        //    }

        //    return this;
        //}

        public Action OnDiceRoll(Dice dice)
        {
            _modStore!.Add(BaseModifier.Create(this, dice, x => x.DiceRoll));
            return this;
        }

        public Action OnDiceRoll(string name, Dice dice, Expression<Func<Func<Dice, Dice>>>? diceCalc = null)
        {
            _modStore!.Add(BaseModifier.Create(this, name, dice, x => x.DiceRoll, diceCalc));
            return this;
        }

        public Action OnDiceRoll<T, TR>(T source, Expression<Func<T, TR>> sExpr, Expression<Func<Func<Dice, Dice>>>? diceCalc = null)
            where T : Entity
        {
            _modStore!.Add(BaseModifier.Create(source, sExpr, this, x => x.DiceRoll, diceCalc));
            return this;
        }

        public Action OnDiceRollTarget(Dice dice, Expression<Func<Func<Dice, Dice>>>? diceCalc = null)
        {
            _modStore!.Add(BaseModifier.Create(this, dice, x => x.DiceRollTarget, diceCalc));
            return this;
        }

        public Action OnDiceRollTarget<T, TR>(T source, Expression<Func<T, TR>> sExpr, Expression<Func<Func<Dice, Dice>>>? diceCalc = null)
            where T : Entity
        {
            _modStore!.Add(BaseModifier.Create(source, sExpr, this, x => x.DiceRollTarget, diceCalc));
            return this;
        }
    }
}
