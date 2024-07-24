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

        internal RpgArg(ParameterInfo parameterInfo)
        {
            Name = parameterInfo.Name!;
            TypeName = parameterInfo.ParameterType.Name!;
            QualifiedTypeName = parameterInfo.ParameterType.AssemblyQualifiedName!;
            IsNullable = Nullable.GetUnderlyingType(parameterInfo.ParameterType) != null;
        }
    }
}
