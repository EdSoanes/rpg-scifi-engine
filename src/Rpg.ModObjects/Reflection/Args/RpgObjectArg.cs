using System.Reflection;
using Newtonsoft.Json;

namespace Rpg.ModObjects.Reflection.Args
{
    public class RpgObjectArg : RpgArg
    {
        [JsonProperty] public string? ForArgName { get; init; }
        [JsonProperty] public string Archetype { get; init; }

        [JsonConstructor] private RpgObjectArg() { }

        internal RpgObjectArg(ParameterInfo parameterInfo)
            : base(parameterInfo)
        {
            Archetype = parameterInfo.ParameterType.Name;
        }

        internal RpgObjectArg(ParameterInfo parameterInfo, string validArgName)
            : this(parameterInfo)
        { 
            ForArgName = validArgName;
        }

        public override bool IsValid(string argName, object? value)
            => ValidType(value) && (ForArgName == null || ForArgName == argName);

        public override string? ToArgString(RpgGraph graph, object? value)
            => ValidType(value)
                ? (value as RpgObject)?.Id
                : null;

        public override object? FromInput(RpgGraph graph, object? value)
        {
            if (value is string id)
            {
                var res = graph.GetObject(id);
                if (ValidType(res))
                    return res;

                throw new ArgumentException($"value {value} invalid");

            }

            return ValidType(value)
                ? value
                : null;
        }

        public override object? ToOutput(RpgGraph graph, object? value)
            => ValidType(value)
                ? (value as RpgObject)!.Id
                : null;

        private bool ValidType(object? value)
        {
            if (value is RpgObject obj)
                return obj.Archetypes.Contains(Archetype);

            return false;
        }
    }
}
