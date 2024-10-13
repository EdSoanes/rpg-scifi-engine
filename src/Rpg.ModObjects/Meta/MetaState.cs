using Rpg.ModObjects.Meta.Attributes;
using System.Reflection;
using Newtonsoft.Json;

namespace Rpg.ModObjects.Meta
{
    public class MetaState
    {
        [JsonProperty] public string Name { get; private set; }
        [JsonProperty] public string Archetype { get; private set; }
        [JsonProperty] public bool Required { get; private set; }
        [JsonProperty] public bool Hidden { get; private set; }
        [JsonProperty] public string? Category { get; private set; }
        [JsonProperty] public string? SubCategory { get; private set; }

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
