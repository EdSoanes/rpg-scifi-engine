using System.Reflection;
using System.Text.Json.Serialization;

namespace Rpg.ModObjects.Reflection.Args
{
    public abstract class RpgArg
    {
        [JsonInclude] public string Name { get; internal set; }
        [JsonInclude] public string TypeName { get; internal set; }
        [JsonInclude] public string QualifiedTypeName { get; internal set; }
        [JsonInclude] public bool IsNullable { get; internal set; }

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
