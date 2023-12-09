using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Components;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.Modifiers;
using System.Linq.Expressions;

namespace Rpg.SciFi.Engine.Artifacts.Turns
{
    public class TurnAction : Entity
    {
        [JsonProperty] private int? Resolution { get; set; }
        [JsonProperty] private int? TargetResolution { get; set; }

        [JsonConstructor] private TurnAction() { }

        public TurnAction(string name, int actionCost, int exertionCost, int focusCost)
        {
            Name = name;
            BaseAction = actionCost;
            BaseExertion = exertionCost;
            BaseFocus = focusCost;

            MetaData.Meta.Add(this);
        }

        [JsonProperty] public string Name { get; private set; }
        [JsonProperty] public TurnPoints Costs { get; private set; }
        [JsonProperty] public int BaseAction { get; protected set; }
        [JsonProperty] public int BaseExertion { get; protected set; }
        [JsonProperty] public int BaseFocus { get; protected set; }

        [Moddable] public int Action { get => this.Resolve(nameof(Action)); }
        [Moddable] public int Exertion { get => this.Resolve(nameof(Exertion)); }
        [Moddable] public int Focus { get => this.Resolve(nameof(Focus)); }

        [Moddable] public Dice DiceRoll { get => Evaluate(nameof(DiceRoll)); }
        [Moddable] public Dice DiceRollTarget { get => Evaluate(nameof(DiceRollTarget)); }
        [Moddable] public Modifier[] Success { get => Meta.Mods.Get(nameof(Success)).ToArray(); }
        [Moddable] public Modifier[] Failure { get => Meta.Mods.Get(nameof(Failure)).ToArray(); }

        public bool IsResolved { get => Resolution != null; }

        private TurnAction? SuccessAction { get; set; }
        private TurnAction? FailureAction { get; set; }

        [Setup]
        public void Setup()
        {
            this.Mod((x) => BaseAction, (x) => Action).IsBase().Apply();
            this.Mod((x) => BaseExertion, (x) => Exertion).IsBase().Apply();
            this.Mod((x) => BaseFocus, (x) => Focus).IsBase().Apply();
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

            return res ?? new Modifier[0];
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
            if (!IsResolved)
                Meta.Mods.Store(nameof(Success), mod);

            return this;
        }

        public TurnAction OnFailure(Modifier mod)
        {
            if (!IsResolved)
                Meta.Mods.Store(nameof(Failure), mod);

            return this;
        }

        public TurnAction OnSuccessAction(Modifier mod, string? name = null, int actionCost = 0, int exertionCost = 0, int focusCost = 0)
        {
            if (!IsResolved)
            {
                if (SuccessAction == null && !string.IsNullOrEmpty(name))
                    SuccessAction = new TurnAction(name, actionCost, exertionCost, focusCost);

                if (SuccessAction == null)
                    throw new ArgumentException($"{nameof(OnSuccessAction)} invalid. No {nameof(SuccessAction)}", nameof(OnSuccessAction));

                SuccessAction.Meta.Mods.Add(mod);
            }

            return this;
        }

        public TurnAction OnFailureAction(Modifier mod, string? name = null, int actionCost = 0, int exertionCost = 0, int focusCost = 0)
        {
            if (!IsResolved)
            {
                if (FailureAction == null && !string.IsNullOrEmpty(name))
                    FailureAction = new TurnAction(name, actionCost, exertionCost, focusCost);

                if (FailureAction == null)
                    throw new ArgumentException($"{nameof(OnSuccessAction)} invalid. No {nameof(FailureAction)}", nameof(OnFailureAction));

                FailureAction.Meta.Mods.Add(mod);
            }

            return this;
        }

        public TurnAction OnDiceRoll(Dice dice)
        {
            this.Mod("Base", dice, (x) => x.DiceRoll)
                .IsInstant()
                .Apply();

            return this;
        }

        public TurnAction OnDiceRoll(string name, Dice dice, Expression<Func<Func<Dice, Dice>>>? diceCalc = null)
        {
            this.Mod(name, dice, (x) => x.DiceRoll, diceCalc)
                .IsInstant()
                .Apply();

            return this;
        }

        public TurnAction OnDiceRoll<T, TR>(T source, Expression<Func<T, TR>> sExpr, Expression<Func<Func<Dice, Dice>>>? diceCalc = null)
            where T : Entity
        {
            source.Mod(sExpr, this, (x) => x.DiceRoll, diceCalc)
                .IsInstant()
                .Apply();

            return this;
        }

        public TurnAction OnDiceRollTarget(Dice dice, Expression<Func<Func<Dice, Dice>>>? diceCalc = null)
        {
            this.Mod("Base", dice, (x) => x.DiceRollTarget, diceCalc)
                .IsInstant()
                .Apply();

            return this;
        }

        public TurnAction OnDiceRollTarget<T, TR>(T source, Expression<Func<T, TR>> sExpr, Expression<Func<Func<Dice, Dice>>>? diceCalc = null)
            where T : Entity
        {
            source.Mod(sExpr, this, (x) => x.DiceRollTarget, diceCalc)
                .IsInstant()
                .Apply();

            return this;
        }
    }
}
