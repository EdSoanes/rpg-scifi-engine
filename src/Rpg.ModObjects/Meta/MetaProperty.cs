using Newtonsoft.Json;
using System.Reflection;

namespace Rpg.ModObjects.Meta
{
    public class MetaProperty
    {
        [JsonProperty] public string Name { get; private set; }
        [JsonProperty] public string ReturnType { get; private set; }
        [JsonProperty] public string? FullReturnType { get; private set; }
        [JsonProperty] public bool IsComponent { get; internal set; }
        [JsonProperty] public MetaPropUI UI { get; internal set; }
        [JsonConstructor] private MetaProperty() { }

        public MetaProperty(PropertyInfo propertyInfo)
        {
            Name = propertyInfo.Name;
            ReturnType = propertyInfo.PropertyType.Name;
            FullReturnType = propertyInfo.PropertyType.AssemblyQualifiedName;

            var attr = propertyInfo.GetCustomAttribute<MetaPropUIAttribute>(true);
            if (attr != null)
                UI = new MetaPropUI
                {
                    Editor = attr.Editor,
                    Keys = attr.Keys,
                    Max = attr.Max,
                    Min = attr.Min,
                    Unit = attr.Unit
                };
        }

        public override string ToString()
        {
            return $"{Name} ({ReturnType})";
        }
    }
}
