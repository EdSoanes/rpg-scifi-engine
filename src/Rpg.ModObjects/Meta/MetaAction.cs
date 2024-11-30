using Rpg.ModObjects.Meta.Attributes;
using Rpg.ModObjects.Reflection;
using System.Reflection;
using Newtonsoft.Json;
using Rpg.ModObjects.Activities;
using Rpg.ModObjects.Mods;

namespace Rpg.ModObjects.Meta
{
    public class MetaAction
    {
        [JsonProperty] public string Name { get; private set; }
        [JsonProperty] public string OwnerArchetype { get; private set; }
        [JsonProperty] public bool Required { get; private set; }
        [JsonProperty] public string? Category { get; private set; }
        [JsonProperty] public string? SubCategory { get; private set; }
        [JsonProperty] public string[]? NextActionHints { get; private set; }

        [JsonProperty] private RpgMethod<ActionTemplate, bool>? Cost { get; set; }
        [JsonProperty] private RpgMethod<ActionTemplate, bool>? Perform { get; set; }
        [JsonProperty] private RpgMethod<ActionTemplate, bool> Outcome { get; set; }

        [JsonConstructor] private MetaAction() { }

        public MetaAction(Type actionType)
        {
            var actionTemplate = (ActionTemplate)Activator.CreateInstance(actionType, true)!;

            Name = actionType.Name;
            //OwnerArchetype =
            Cost = actionTemplate.CostMethod;
            Perform = actionTemplate.PerformMethod;
            Outcome = actionTemplate.OutcomeMethod;

            var attr = actionType.GetCustomAttribute<ActionAttribute>();
            if (attr != null)
            {
                Required = attr.Required;
                Category = attr.Category;
                SubCategory = attr.SubCategory;
                NextActionHints = attr.NextActionHints;
            }
        }

        public override string ToString()
        {
            return $"{Name} ({OwnerArchetype})";
        }
    }
}
