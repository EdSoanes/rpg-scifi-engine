using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Expressions.Parsers;
using System.Text;

namespace Rpg.SciFi.Engine.Artifacts.Expressions
{
    public enum DiceOptions
    {
        None,
        Simplified
    }

    public struct Dice
    {
        public static readonly Dice Zero = new Dice("0");
        private static readonly string Minus = "-";

        private List<IDiceNode> _nodes { get; set; }

        private string _expr = "0";
        [JsonProperty]
        private string Expr
        {
            get { return _expr; }
            set
            {
                _expr = value;
                _nodes = DiceParser.Simplified(_expr);
            }
        }

        public Dice(string expr) => Expr = expr;

        public static implicit operator string(Dice d) => d.ToString();
        public static implicit operator Dice(string expression) => new Dice(expression);
        public static Dice operator +(Dice d1, Dice d2)
        {
            string expr1 = d1;
            string expr2 = d2;

            return expr2.StartsWith("-")
                ? new Dice(expr1 + expr2)
                : new Dice($"{expr1}+{expr2}");
        }

        public static bool operator ==(Dice d1, Dice d2) => d1.ToString() == d2.ToString();
        public static bool operator !=(Dice d1, Dice d2) => d1.ToString() != d2.ToString();

        public int Roll() => _nodes.Sum(x => x.Roll());
        public double Avg() => _nodes.Sum(x => x.Avg());
        public int Min() => _nodes.Sum(x => x.Min());
        public int Max() => _nodes.Sum(x => x.Max());

        public override string ToString()
        {
            if (_nodes == null || !_nodes.Any())
                return "0";

            var sb = new StringBuilder(_nodes.First().ToString());
            foreach (var node in _nodes.Skip(1))
            {
                if (node.Multiplier >= 0)
                    sb.Append(" + ").Append(node.ToString());
                else
                    sb.Append(node.ToString()!.Replace(Minus, $" {Minus} "));
            }

            return sb.ToString();
        }
    }
}
