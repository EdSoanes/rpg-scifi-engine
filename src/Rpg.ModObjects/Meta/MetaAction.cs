using Rpg.ModObjects.Meta.Attributes;
using Rpg.ModObjects.Reflection;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Rpg.ModObjects.Meta
{
    public class MetaAction
    {
        [JsonInclude] public string Name { get; private set; }
        [JsonInclude] public string OwnerArchetype { get; private set; }
        [JsonInclude] public bool Required { get; private set; }
        [JsonInclude] public string? Category { get; private set; }
        [JsonInclude] public string? SubCategory { get; private set; }
        [JsonInclude] public string[]? NextActionHints { get; private set; }

        [JsonInclude] private RpgMethod<Actions.Action, bool> OnCost { get; set; }
        [JsonInclude] private RpgMethod<Actions.Action, bool> OnAct { get; set; }
        [JsonInclude] private RpgMethod<Actions.Action, bool> OnOutcome { get; set; }

        [JsonConstructor] private MetaAction() { }

        public MetaAction(Type actionType)
        {
            var action = (Actions.Action)Activator.CreateInstance(actionType, true)!;

            Name = actionType.Name;
            OwnerArchetype = action.OwnerArchetype;
            OnCost = action.OnCost;
            OnAct = action.OnAct;
            OnOutcome = action.OnOutcome;

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
