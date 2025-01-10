using Rpg.Experimental.Meta.Attributes;
using Rpg.Experimental.Reflection;
using System.Reflection;
using Newtonsoft.Json;
using Rpg.Experimental.Activities;
using Rpg.Experimental.Mods;

namespace Rpg.Experimental.Meta
{
    public class MetaAction
    {
        [JsonProperty] public string Name { get; private set; }
        [JsonProperty] public string OwnerArchetype { get; private set; }
        [JsonProperty] public bool Required { get; private set; }
        [JsonProperty] public string? Category { get; private set; }
        [JsonProperty] public string? SubCategory { get; private set; }
        [JsonProperty] public string[]? NextActionHints { get; private set; }

        [JsonProperty] private RpgMethod<RpgAction, bool>? Cost { get; set; }
        [JsonProperty] private RpgMethod<RpgAction, bool>? Perform { get; set; }
        [JsonProperty] private RpgMethod<RpgAction, bool> Outcome { get; set; }

        [JsonConstructor] private MetaAction() { }

        public MetaAction(Type actionType)
        {
            var actionTemplate = (RpgAction)Activator.CreateInstance(actionType, true)!;

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
