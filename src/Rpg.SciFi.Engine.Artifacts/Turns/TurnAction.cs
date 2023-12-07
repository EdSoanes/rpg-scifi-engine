using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Components;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.Modifiers;
using System.Linq.Expressions;

namespace Rpg.SciFi.Engine.Artifacts.Turns
{
    public class TurnAction : Entity
    {
        [JsonProperty] private ModifierStore _modStore = new ModifierStore();

        [JsonConstructor] private TurnAction() { }

        public TurnAction(int actionCost, int exertionCost, int focusCost)
        {
            Costs = new TurnPoints(actionCost, exertionCost, focusCost);
        }

        [JsonProperty] public TurnPoints Costs { get; private set; }

        [Moddable] public Dice DiceRoll { get => _modStore.Evaluate(nameof(DiceRoll)); }
        [Moddable] public Dice DiceRollTarget { get => _modStore.Evaluate(nameof(DiceRollTarget)); }
        [Moddable] public Modifier[] Success { get => _modStore.Get(nameof(Success)).ToArray(); }
        [Moddable] public Modifier[] Failure { get => _modStore.Get(nameof(Failure)).ToArray(); }

        public override string[] Describe(string prop) => _modStore.Describe(prop, true);

        public TurnAction OnSuccess(Modifier mod)
        {
            _modStore.Store(nameof(Success), mod);
            return this;
        }

        public TurnAction OnFailure(Modifier mod)
        {
            _modStore.Store(nameof(Failure), mod);
            return this;
        }

        public TurnAction OnDiceRoll(Dice dice)
        {
            this.Mod("Base", dice, (x) => x.DiceRoll)
                .IsInstant()
                .Apply(_modStore);

            return this;
        }

        public TurnAction OnDiceRoll(string name, Dice dice, Expression<Func<Func<Dice, Dice>>>? diceCalc = null)
        {
            this.Mod(name, dice, (x) => x.DiceRoll, diceCalc)
                .IsInstant()
                .Apply(_modStore);

            return this;
        }

        public TurnAction OnDiceRoll<T, TR>(T source, Expression<Func<T, TR>> sExpr, Expression<Func<Func<Dice, Dice>>>? diceCalc = null)
            where T : Entity
        {
            source.Mod(sExpr, this, (x) => x.DiceRoll, diceCalc)
                .IsInstant()
                .Apply(_modStore);

            return this;
        }

        public TurnAction OnDiceRollTarget(Dice dice, Expression<Func<Func<Dice, Dice>>>? diceCalc = null)
        {
            this.Mod("Base", dice, (x) => x.DiceRollTarget, diceCalc)
                .IsInstant()
                .Apply(_modStore);

            return this;
        }

        public TurnAction OnDiceRollTarget<T, TR>(T source, Expression<Func<T, TR>> sExpr, Expression<Func<Func<Dice, Dice>>>? diceCalc = null)
            where T : Entity
        {
            source.Mod(sExpr, this, (x) => x.DiceRollTarget, diceCalc)
                .IsInstant()
                .Apply(_modStore);

            return this;
        }
    }
}
