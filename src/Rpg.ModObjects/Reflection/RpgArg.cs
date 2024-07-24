using Newtonsoft.Json;
using Rpg.ModObjects.Values;
using System.Reflection;

namespace Rpg.ModObjects.Reflection
{
    public abstract class RpgArg
    {
        [JsonProperty] public string Name { get; internal set; }
        [JsonProperty] public string TypeName { get; internal set; }
        [JsonProperty] public string QualifiedTypeName { get; internal set; }
        [JsonProperty] public bool IsNullable { get; internal set; }

        public abstract bool IsValid(object? value);
        public abstract string? ToArgString(object? value);
        public abstract object? ToArgObject(string? value);

        [JsonConstructor] protected RpgArg() { }

        internal RpgArg(ParameterInfo parameterInfo)
        {
            Name = parameterInfo.Name!;
            IsNullable = Nullable.GetUnderlyingType(parameterInfo.ParameterType) != null;
            TypeName = !IsNullable ? parameterInfo.ParameterType.Name! : parameterInfo.ParameterType.GetGenericArguments().First().Name;
            QualifiedTypeName = parameterInfo.ParameterType.AssemblyQualifiedName!;
        }
    }
}
