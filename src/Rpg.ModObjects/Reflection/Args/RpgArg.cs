using Newtonsoft.Json;
using System.Reflection;

namespace Rpg.ModObjects.Reflection.Args
{
    public abstract class RpgArg
    {
        [JsonProperty] public string Name { get; internal set; }
        [JsonProperty] public string TypeName { get; internal set; }
        [JsonProperty] public string QualifiedTypeName { get; internal set; }
        [JsonProperty] public bool IsNullable { get; internal set; }

        public abstract bool IsValid(string argName, object? value);
        public abstract string? ToArgString(RpgGraph graph, object? value);
        public abstract object? FromInput(RpgGraph graph, object? value);
        public abstract object? ToOutput(RpgGraph graph, object? value);

        [JsonConstructor] protected RpgArg() { }

        internal RpgArg(ParameterInfo parameterInfo)
        {
            Name = parameterInfo.Name!;
            IsNullable = Nullable.GetUnderlyingType(parameterInfo.ParameterType) != null;
            TypeName = !IsNullable
                ? parameterInfo.ParameterType.Name!
                : parameterInfo.ParameterType.GetGenericArguments().First().Name;
            QualifiedTypeName = !IsNullable
                ? parameterInfo.ParameterType.AssemblyQualifiedName!
                : parameterInfo.ParameterType.GetGenericArguments().First().AssemblyQualifiedName!;
        }
    }
}
