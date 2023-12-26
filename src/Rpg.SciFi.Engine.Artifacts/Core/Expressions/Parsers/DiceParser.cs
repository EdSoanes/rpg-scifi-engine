using Rpg.SciFi.Engine.Artifacts.MetaData;
using System.Text.RegularExpressions;

namespace Rpg.SciFi.Engine.Artifacts.Expressions.Parsers
{
    internal interface IDiceNode
    {
        int Multiplier { get; set; }

        int Roll();
        double Avg();
        int Min();
        int Max();
    }

    internal class NumberNode : IDiceNode
    {
        internal static readonly Regex Token = new Regex("^[0-9]+$");

        public int Multiplier { get; set; }
        public int Number { get; private set; }

        public NumberNode(int multiplier, int number)
        {
            Multiplier = multiplier;
            Number = number;
        }

        public double Avg() => Number * Multiplier;
        public int Roll() => Number * Multiplier;
        public int Min() => Number * Multiplier;
        public int Max() => Number * Multiplier;

        public static IDiceNode? Create(int multiplier, string token)
        {
            if (!Token.IsMatch(token))
                return null;

            if (!int.TryParse(token, out var num))
                throw new ArgumentException($"Invalid number token {token}");

            return num != 0
                ? new NumberNode(multiplier, num)
                : null;
        }

        public override string ToString()
        {
            return $"{(Multiplier < 0 ? "-" : "")}{Number}";
        }
    }

    internal class DiceNode : IDiceNode
    {
        private static readonly Regex Token = new Regex("^([0-9]*)d([0-9]+|%)$");

        private static readonly Random _roller = new Random();

        public int Multiplier { get; set; }
        public int NoOfDice { get; private set; }
        public int DiceType { get; private set; }

        public DiceNode(int multiplier, int noOfDice, int diceType)
        {
            Multiplier = multiplier;
            DiceType = diceType;
            NoOfDice = noOfDice;
        }

        public static IDiceNode? Create(int multiplier, string token)
        {
            if (!Token.IsMatch(token))
                return null;

            var match = Token.Match(token);

            return new DiceNode(
                multiplier,
                match.Groups[1].Value == string.Empty ? 1 : int.Parse(match.Groups[1].Value),
                match.Groups[2].Value == "%" ? 100 : int.Parse(match.Groups[2].Value));
        }

        public double Avg() => Multiplier * NoOfDice * ((DiceType + 1.0) / 2.0);

        public int Roll()
        {
            int total = 0;
            for (int i = 0; i < NoOfDice; ++i)
                total += _roller.Next(1, DiceType + 1);

            return Multiplier * total;
        }

        public int Min() => Multiplier >= 0 ? NoOfDice : Multiplier * NoOfDice * DiceType;
        public int Max() => Multiplier >= 0 ? NoOfDice * DiceType : Multiplier * NoOfDice;

        public override string ToString()
        {
            return $"{(Multiplier < 0 ? "-" : "")}{NoOfDice}d{DiceType}";
        }
    }

    internal static class DiceParser
    {
        private static readonly Regex DiceExpressionToken = new Regex("\\[[a-zA-Z.]*?\\]");

        public static List<IDiceNode> Simplified(string expr)
        {
            var nodes = Parse(expr);

            List<IDiceNode> diceNodes = nodes
                .Where(node => node.GetType() == typeof(DiceNode))
                .Cast<DiceNode>()
                .GroupBy(x => x.DiceType)
                .OrderByDescending(x => x.Key)
                .Select(x =>
                {
                    var noOfDice = x.Sum(n => n.Multiplier * n.NoOfDice);
                    var multiplier = noOfDice < 0 ? -1 : 1;
                    return new DiceNode(multiplier, Math.Abs(noOfDice), x.Key);
                })
                .ToList<IDiceNode>();

            var number = nodes
                .Where(node => node.GetType() == typeof(NumberNode))
                .Cast<NumberNode>()
                .Sum(node => node.Multiplier * node.Number);

            if (number != 0)
                diceNodes.Add(new NumberNode(number < 0 ? -1 : 1, Math.Abs(number)));

            return diceNodes;
        }

        public static List<IDiceNode> Parse(string expr)
        {
            var res = new List<IDiceNode>();

            string[] tokens = Tokenize(expr);

            // Parse operator-then-operand pairs into this.nodes.
            for (int tokenIndex = 0; tokenIndex < tokens.Length; tokenIndex += 2)
            {
                var token = tokens[tokenIndex];
                var nextToken = tokens[tokenIndex + 1];

                int multiplier = GetMultiplier(expr, token);

                var node = NumberNode.Create(multiplier, nextToken)
                    ?? DiceNode.Create(multiplier, nextToken);

                if (node != null)
                {
                    res.Add(node);
                }
                else if (DiceExpressionToken.IsMatch(nextToken))
                {
                    //TODO: Fix dice expressions
                    //var subExpr = Meta.ValueByPath<string>(nextToken.Replace("[", "").Replace("]", ""));
                    var subExpr = nextToken.Replace("[", "").Replace("]", "");

                    var subNodes = Parse(subExpr!);
                    if (subNodes.Any())
                    {
                        multiplier = subNodes.First().Multiplier * multiplier;
                        subNodes.First().Multiplier = multiplier;

                        res.AddRange(subNodes);
                    }
                }
            }

            return res;
        }

        private static int GetMultiplier(string expr, string token)
        {
            if (token != "+" && token != "-")
                throw new ArgumentException($"{expr} contains invalid token {token}. Expecting + or -");

            int multiplier = token == "+" ? +1 : -1;
            return multiplier;
        }

        private static string[] Tokenize(string expr)
        {
            var tokens = expr?
                .Replace("+", " + ")
                .Replace("-", " - ")
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                ?? new[] { "0" };

            // Blank dice expressions end up being DiceExpression.Zero.
            if (!tokens.Any())
                tokens = new[] { "0" };

            // Since we parse tokens in operator-then-operand pairs, make sure the first token is an operand.
            if (tokens[0] != "+" && tokens[0] != "-")
                tokens = (new[] { "+" }).Concat(tokens).ToArray();

            // This is a precondition for the below parsing loop to make any sense.
            if (tokens.Length % 2 != 0)
                throw new ArgumentException($"The given dice expression '{expr}' was not in an expected format: even after normalization, it contained an odd number of tokens.");

            return tokens;
        }
    }
}
