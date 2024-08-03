using Newtonsoft.Json;
using System.Reflection;

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
            => ToArgValue(graph, value)?.ToString();

        public override object? ToArgObject(RpgGraph graph, string? value)
            => ToArgValue(graph, value);

        public override object? ToArgValue(RpgGraph graph, object? value)
            => int.TryParse(value?.ToString(), out int result)
                ? (object?)result
                : null;
    }
}
