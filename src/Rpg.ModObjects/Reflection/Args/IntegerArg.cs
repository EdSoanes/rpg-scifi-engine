using Rpg.ModObjects.Values;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Rpg.ModObjects.Reflection.Args
{
    public class IntegerArg : RpgArg
    {
        [JsonConstructor] private IntegerArg() { }

        public IntegerArg(ParameterInfo parameterInfo)
            : base(parameterInfo)
        { }

        public override bool IsValid(string argName, object? value)
            => int.TryParse(value?.ToString(), out int _);

        public override string? ToArgString(RpgGraph graph, object? value)
            => ToOutput(graph, value)?.ToString();

        public override object? FromInput(RpgGraph graph, object? value)
        {
            if (value is null) return null;
            if (value is int i) return i;
            if (value is Dice dice && dice.IsConstant)
                return dice.Roll();
            else if (int.TryParse(value.ToString(), out var i2))
                return i2;

            throw new ArgumentException($"value {value} invalid");
        }

        public override object? ToOutput(RpgGraph graph, object? value)
        {
            if (value is null) return null;
            if (value is int i) return i;
            if (value is Dice dice)
            {
                if (dice.IsConstant)
                    return dice.Roll();
                else
                    return dice;
            }

            else if (int.TryParse(value.ToString(), out var i2))
                return i2;

            throw new ArgumentException($"value {value} invalid");
        }

    }
}
