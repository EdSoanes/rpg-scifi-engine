using Newtonsoft.Json;
using System.Reflection;

namespace Rpg.ModObjects.Reflection.ArgTypes
{
    public class ObjectArgType : IRpgArgType
    {
        [JsonProperty] public string TypeName { get; private set; }
        [JsonProperty] public string QualifiedTypeName { get; private set; }
        [JsonProperty] public bool IsNullable { get; set; } = true;

        public bool IsArgTypeFor(ParameterInfo parameterInfo)
            => (parameterInfo.ParameterType.IsClass || parameterInfo.ParameterType.IsInterface) 
                && !parameterInfo.ParameterType.IsAssignableTo(typeof(RpgObject));

        public IRpgArgType Clone(Type? type = null)
        {
            var clone = Activator.CreateInstance<ObjectArgType>();
            clone.IsNullable = IsNullable;
            clone.TypeName = type?.Name ?? TypeName;
            clone.QualifiedTypeName = type?.AssemblyQualifiedName ?? QualifiedTypeName;
            return clone;
        }

        public bool IsValid(object? value)
            => true;

        public string? ToArgString(object? value)
            => value?.ToString();

        public object? ToArgObject(string? value) 
            => null;
    }
}
