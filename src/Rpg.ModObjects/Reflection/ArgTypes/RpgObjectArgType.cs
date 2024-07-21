using Newtonsoft.Json;
using Rpg.ModObjects.Values;
using System.Reflection;

namespace Rpg.ModObjects.Reflection.ArgTypes
{
    public class RpgObjectArgType : IRpgArgType
    {
        [JsonProperty] public string TypeName { get; private set; }
        [JsonProperty] public string QualifiedTypeName { get; private set; }
        [JsonProperty] public bool IsNullable { get; set; } = true;

        public bool IsArgTypeFor(ParameterInfo parameterInfo)
            => parameterInfo.ParameterType.IsAssignableTo(typeof(RpgObject));

        public IRpgArgType Clone(Type? type = null)
        {
            var clone = Activator.CreateInstance<RpgObjectArgType>();
            clone.IsNullable = IsNullable;
            clone.TypeName = type?.Name ?? TypeName;
            clone.QualifiedTypeName = type?.AssemblyQualifiedName ?? QualifiedTypeName;
            return clone;
        }

        public bool IsValid(object? value)
            => Dice.TryParse(value?.ToString(), out Dice _);

        public string? ToArgString(object? value)
            => (value as RpgObject)?.Id;

        public object? ToArgObject(string? value)
            => value;
    }
}
