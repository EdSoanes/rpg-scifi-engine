using Rpg.ModObjects.Meta.Attributes;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Rpg.ModObjects.Meta
{
    public class MetaState
    {
        [JsonInclude] public string Name { get; private set; }
        [JsonInclude] public string Archetype { get; private set; }
        [JsonInclude] public bool Required { get; private set; }
        [JsonInclude] public bool Hidden { get; private set; }
        [JsonInclude] public string? Category { get; private set; }
        [JsonInclude] public string? SubCategory { get; private set; }

        [JsonConstructor] private MetaState() { }

        public MetaState(Type stateType)
        {
            Name = stateType.Name;
            Archetype = stateType.BaseType!.GenericTypeArguments[0].Name;

            var attr = stateType.GetCustomAttribute<StateAttribute>();
            if (attr != null)
            {
                Required = attr.Required;
                Hidden = attr.Hidden;
                Category = attr.Category;
                SubCategory = attr.SubCategory;
            }
        }

        public override string ToString()
        {
            return $"{Name} ({Archetype})";
        }
    }
}
