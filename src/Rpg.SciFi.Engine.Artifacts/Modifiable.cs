using Newtonsoft.Json;

namespace Rpg.SciFi.Engine.Artifacts
{
    public sealed class Modification
    {
        private int? _evaluatedDiceRoll = null;
        [JsonProperty] public Guid Id { get; protected set; } = Guid.NewGuid();
        [JsonProperty] public string Name { get; set; }
        [JsonProperty] public string Target { get; set; }
        [JsonProperty] public string DiceExpr { get; set; }

        public int DiceEval(object context)
        {
            if (_evaluatedDiceRoll == null)
            {
                var dice = new DiceExpression(DiceExpr);
                _evaluatedDiceRoll = dice.Roll(context);
            }

            return _evaluatedDiceRoll.Value;
        }

        public string DiceExpression(object context, string diceExpr)
        {
            var prop = Target.Split('.').Last();
            var expr = !string.IsNullOrEmpty(DiceExpr) && !DiceExpr.StartsWith('-')
                ? $"{diceExpr}+{DiceExpr}"
                : $"{diceExpr}{DiceExpr}";

            var dice = new DiceExpression(expr, DiceExpressionOptions.SimplifyStringValue);
            return dice.ToString();
        }
    }

    public abstract class BaseModifiable
    {
        public BaseModifiable(Guid? id = null, string? name = null)
        {
            Id = id ?? Guid.NewGuid();
            Name = name ?? GetType().Name;
        }

        public Guid Id { get; }
        public string Name { get; }
    }

    public class Modifiable<T> where T : class, new()
    {
        [JsonProperty] public Guid Id { get; private set; } = Guid.NewGuid();
        [JsonProperty] public string Name { get; protected set; } = nameof(Modifiable<T>);
        [JsonProperty] protected List<Modification> Modifications { get; private set; } = new List<Modification>();
        [JsonProperty] protected T BaseModel { get; private set; } = new T();

        public string DiceExpr(string prop, string? diceExpr = null)
        {
            var exprs = Modifications.Where(x => x.Target == prop).Select(x => x.DiceExpr);
            var res = !string.IsNullOrEmpty(diceExpr)
                ? diceExpr
                :string.Empty;

            foreach (var expr in exprs)
                if (!expr.StartsWith('-') && !expr.StartsWith('+'))
                    res += $"+{expr}";

            var dice = new DiceExpression(res, DiceExpressionOptions.SimplifyStringValue);
            return dice.Expression(this);
        }

        public void ClearModifications()
        {
            Modifications.Clear();
        }

        public void AddModification(Modification modification)
        {
            if (!Modifications.Any(x => x.Id == modification.Id))
                Modifications.Add(modification);
        }

        public void RemoveModification(Guid id)
        {
            var modification = Modifications.SingleOrDefault(x => x.Id == id);
            if (modification != null)
                Modifications.Remove(modification);
        }
    }
}
