using Newtonsoft.Json;
using System.Reflection;

namespace Rpg.ModObjects.Reflection
{
    public class RpgArg
    {
        [JsonProperty] public string Name { get; private set; }
        [JsonProperty] public IRpgArgType ArgType { get; private set; }

        [JsonProperty] public string TypeName { get; private set; }
        [JsonProperty] internal string QualifiedTypeName { get; private set; }
        [JsonProperty] public bool IsNullable { get; private set; }

        [JsonConstructor] private RpgArg() { }

        internal RpgArg(ParameterInfo parameterInfo)
        {
            Name = parameterInfo.Name!;
            TypeName = parameterInfo.ParameterType.Name!;
            QualifiedTypeName = parameterInfo.ParameterType.AssemblyQualifiedName!;
            IsNullable = Nullable.GetUnderlyingType(parameterInfo.ParameterType) != null;
        }

        public RpgArg Clone()
            => new RpgArg
            {
                Name = Name,
                TypeName = TypeName,
                QualifiedTypeName = QualifiedTypeName,
                IsNullable = IsNullable
            };
    }
}
