using System.Reflection;
using Newtonsoft.Json;

namespace Rpg.ModObjects.Reflection.Args
{
    public class DefaultArg : RpgArg
    {
        [JsonConstructor] private DefaultArg() { }

        public DefaultArg(ParameterInfo parameterInfo) 
            : base(parameterInfo)
        { }

        public override bool IsValid(string argName, object? value)
            => true;

        public override string? ToArgString(RpgGraph graph, object? value)
            => value?.ToString();

        public override object? FromInput(RpgGraph graph, object? value)
        {
            if (value == null) return null;
            if (RpgMethodArgs.PrimitiveArgTypes.Contains(value?.GetType())) return value;

            throw new ArgumentException($"value {value} invalid");

        }

        public override object? ToOutput(RpgGraph graph, object? value)
        {
            if (value == null) return null;
            if (RpgMethodArgs.PrimitiveArgTypes.Contains(value?.GetType())) return value;

            throw new ArgumentException($"value {value} invalid");

        }
    }
}
