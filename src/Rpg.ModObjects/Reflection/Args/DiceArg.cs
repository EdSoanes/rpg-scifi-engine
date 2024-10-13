using Rpg.ModObjects.Values;
using System.Reflection;
using Newtonsoft.Json;
using System.Windows.Markup;

namespace Rpg.ModObjects.Reflection.Args
{
    public class DiceArg : RpgArg
    {
        [JsonConstructor] private DiceArg() { }

        public DiceArg(ParameterInfo parameterInfo) 
            : base(parameterInfo)
        { }

        public override bool IsValid(string argName, object? value)
            => Dice.TryParse(value?.ToString(), out Dice _);

        public override string? ToArgString(RpgGraph graph, object? value)
            => ToOutput(graph, value)?.ToString();

        public override object? FromInput(RpgGraph graph, object? value)
            => Convert(graph, value);

        public override object? ToOutput(RpgGraph graph, object? value)
            => Convert(graph, value);

        private Dice? Convert(RpgGraph graph, object? value)
        {
            if (value == null) return null;
            if (value is Dice dice) return dice;
            if (Dice.TryParse(value?.ToString(), out Dice result)) return result;

            throw new ArgumentException($"value {value} invalid");

        }
    }
}
