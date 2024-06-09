using Newtonsoft.Json;
using Rpg.ModObjects.Props;
using Rpg.ModObjects.Values;
using System.Reflection;

namespace Rpg.ModObjects.Meta
{
    public class MetaProperty
    {
        [JsonProperty] public string Name { get; private set; }
        [JsonProperty] public string ReturnType { get; private set; }
        [JsonProperty] public string? FullReturnType { get; private set; }
        [JsonProperty] public bool IsComponent { get; private set; }
        [JsonProperty] public MetaPropUIAttribute UI { get; private set; }

        [JsonConstructor] private MetaProperty() { }

        public MetaProperty(PropertyInfo propertyInfo)
        {
            Name = propertyInfo.Name;
            ReturnType = propertyInfo.PropertyType.Name;
            FullReturnType = propertyInfo.PropertyType.AssemblyQualifiedName;

            var ui = propertyInfo.GetCustomAttributes(true)
                .FirstOrDefault(x => x.GetType().IsAssignableTo(typeof(MetaPropUIAttribute))) as MetaPropUIAttribute;

            if (ui == null)
            {
                ui = ReturnType switch
                {
                    nameof(Int32) => new IntegerUIAttribute(),
                    nameof(Dice) => new DiceUIAttribute(),
                    nameof(String) => new TextUIAttribute(),
                    _ => new ComponentUIAttribute()
                };
            }

            UI = ui;
            IsComponent = UI is ComponentUIAttribute;
        }

        public override string ToString()
        {
            return $"{Name} ({ReturnType})";
        }
    }
}
