using Newtonsoft.Json;
using Rpg.ModObjects.Values;
using System.Reflection;

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
            => ToArgValue(graph, value)?.ToString();

        public override object? ToArgObject(RpgGraph graph, string? value)
            => ToArgValue(graph, value);

        public override object? ToArgValue(RpgGraph graph, object? value)
            => Dice.TryParse(value?.ToString(), out Dice result)
                ? (object?)result
                : null;
    }
}
