using Newtonsoft.Json;
using System.Reflection;

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

        public override object? ToArgObject(RpgGraph graph, string? value)
            => null;

        public override object? ToArgValue(RpgGraph graph, object? value)
            => value;
    }
}
