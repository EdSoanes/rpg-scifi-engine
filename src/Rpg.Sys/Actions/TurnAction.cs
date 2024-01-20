using Newtonsoft.Json;
using Rpg.Sys.Archetypes;
using Rpg.Sys.Modifiers;
using System.Linq.Expressions;

namespace Rpg.Sys.Actions
{
    public class TurnAction : ActionBase
    {
        [JsonProperty] private int? ResolutionTarget { get; set; }

        [JsonProperty] public List<Modifier> Success { get; private set; } = new List<Modifier>();
        [JsonProperty] public List<Modifier> Failure { get; private set; } = new List<Modifier>();

        public Dice DiceRoll { get; private set; }
        public Dice DiceRollTarget { get; private set; }

        private TurnAction? SuccessAction { get; set; }
        private TurnAction? FailureAction { get; set; }

        public TurnAction(string name, ActionCost actionCost)
            : base(name, actionCost)
        {
        }

        public TurnAction(string name, int actionCost, int exertionCost, int focusCost)
            : base(name, new ActionCost(actionCost, exertionCost, focusCost))
        {
        }

        protected override void OnResolve(Actor actor, Graph graph)
        {
            ResolutionTarget = DiceRollTarget.Roll();
            var modifiers = Resolution >= ResolutionTarget
                ? Success
                : Failure;

            if (modifiers != null)
                graph.Add.Mods(modifiers.ToArray());
        }

        protected override ActionBase? NextAction()
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

        public TurnAction OnSuccess(params Modifier[] mods)
        {
            if (!IsResolved)
            {
                foreach (var mod in mods.Where(x => !Success.Any(y => y.Id == x.Id)))
                    Success.Add(mod);
            }

            return this;
        }

        public TurnAction OnFailure(params Modifier[] mods)
        {
            if (!IsResolved)
            {
                foreach (var mod in mods.Where(x => !Failure.Any(y => y.Id == x.Id)))
                    Failure.Add(mod);
            }

            return this;
        }

        public TurnAction OnDiceRoll(Graph graph, Dice dice)
        {
            graph.Add.Mods(BaseModifier.Create(this, dice, x => x.DiceRoll));
            return this;
        }

        public TurnAction OnDiceRoll(Graph graph, string name, Dice dice, Expression<Func<Func<Dice, Dice>>>? diceCalc = null)
        {
            graph.Add.Mods(BaseModifier.Create(this, name, dice, x => x.DiceRoll, diceCalc));
            return this;
        }

        public TurnAction OnDiceRoll<T, TR>(Graph graph, T source, Expression<Func<T, TR>> sExpr, Expression<Func<Func<Dice, Dice>>>? diceCalc = null)
            where T : ModdableObject
        {
            graph.Add.Mods(BaseModifier.Create(source, sExpr, this, x => x.DiceRoll, diceCalc));
            return this;
        }

        public TurnAction OnDiceRollTarget(Graph graph, Dice dice, Expression<Func<Func<Dice, Dice>>>? diceCalc = null)
        {
            graph.Add.Mods(BaseModifier.Create(this, dice, x => x.DiceRollTarget, diceCalc));
            return this;
        }

        public TurnAction OnDiceRollTarget<T, TR>(Graph graph, T source, Expression<Func<T, TR>> sExpr, Expression<Func<Func<Dice, Dice>>>? diceCalc = null)
            where T : ModdableObject
        {
            graph.Add.Mods(BaseModifier.Create(source, sExpr, this, x => x.DiceRollTarget, diceCalc));
            return this;
        }
    }
}
