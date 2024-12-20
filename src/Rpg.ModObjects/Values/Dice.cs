﻿using System.Text;
using Newtonsoft.Json;

namespace Rpg.ModObjects.Values
{
    public struct Dice
    {
        public static readonly Dice Zero = new Dice("0");
        private static readonly string Minus = "-";

        private string _expr = "0";

        [JsonProperty]
        private string Expr
        {
            get => _expr;
            set
            {
                _nodes = DiceParser.Simplified(_expr);
                _expr = ToString();
            }
        }

        private List<IDiceNode> _nodes { get; set; } = new List<IDiceNode>();
        
        public Dice() { } 
        public Dice(string? expr) => Expr = expr ?? "0";
        public Dice(int val) => Expr = val.ToString();

        public static implicit operator string(Dice d) => d.ToString();
        public static implicit operator Dice(string? expr) => new Dice(expr);
        public static implicit operator Dice(int val) => new Dice(val.ToString());

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

        public bool IsConstant { get => _nodes?.All(x => x.Min() == x.Max()) ?? true; }
        public int Roll() => _nodes?.Sum(x => x.Roll()) ?? 0;
        public double Avg() => _nodes?.Sum(x => x.Avg()) ?? 0;
        public int Min() => _nodes?.Sum(x => x.Min()) ?? 0;
        public int Max() => _nodes?.Sum(x => x.Max()) ?? 0;
        public Dice Negate()
        {
            var dice = new Dice(_expr);
            foreach (var node in dice._nodes)
                node.Multiplier = node.Multiplier < 0 ? 0 : -1;

            return dice;
        }

        public static bool TryParse(object? obj, out Dice dice)
            => TryParse(obj?.ToString(), out dice);

        public static bool TryParse(string? expr, out Dice dice)
        {
            try
            {
                if (string.IsNullOrEmpty(expr))
                {
                    dice = Dice.Zero;
                    return false;
                }

                dice = new Dice(expr);
                return true;
            }
            catch 
            {
                dice = Dice.Zero;
                return false;
            }
        }

        public static Dice Sum(IEnumerable<Dice> dice)
        {
            var res = new Dice();
            foreach (var d in dice.Where(x => !x.IsConstant || x.Roll() != 0))
                res += d;

            return res;
        }

        public static Dice? Add(Dice? d1, Dice? d2)
        {
            if (d1 != null && d2 != null)
                return d1.Value + d2.Value;

            if (d1 == null && d2 == null)
                return null;

            return d1 ?? d2;
        }

        public override string ToString()
        {
            if (_nodes == null || !_nodes.Any())
                return "0";

            var nodes = _nodes.Where(x => !(x is NumberNode) || x.Roll() != 0);
            var sb = new StringBuilder(nodes.First().ToString());
            foreach (var node in nodes.Skip(1))
            {
                if (node.Multiplier >= 0)
                    sb.Append(" + ").Append(node.ToString());
                else
                    sb.Append(node.ToString()!.Replace(Minus, $" {Minus} "));
            }

            return sb.ToString();
        }

        public override bool Equals(object? obj)
        {
            return obj != null
                && (obj is Dice) 
                && ((Dice)obj).ToString() == ToString();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
