using Newtonsoft.Json;
using Rpg.ModObjects.Meta.Attributes;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Reflection;
using System.Reflection;

namespace Rpg.ModObjects.Meta
{
    public class MetaAction
    {
        [JsonProperty] public string Name { get; private set; }
        [JsonProperty] public string OwnerArchetype { get; private set; }
        [JsonProperty] public bool Required { get; private set; }

        [JsonProperty] private RpgMethod<Actions.Action, ModSet> OnCost { get; set; }
        [JsonProperty] private RpgMethod<Actions.Action, ModSet[]> OnAct { get; set; }
        [JsonProperty] private RpgMethod<Actions.Action, ModSet[]> OnOutcome { get; set; }

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
                Required = attr.Required;
        }

        public override string ToString()
        {
            return $"{Name} ({OwnerArchetype})";
        }
    }
}
