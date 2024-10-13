using Rpg.ModObjects.Reflection;
using Newtonsoft.Json;

namespace Rpg.ModObjects.Meta
{
    public class MetaObj
    {
        [JsonProperty] public string Archetype { get; private set; }
        [JsonProperty] public string? QualifiedClassName { get; private set; }
        [JsonProperty] public string[] Archetypes {  get; private set; }
        [JsonProperty] public string? Icon { get; private set; }
        [JsonProperty] public List<MetaProp> Props { get; set; } = new List<MetaProp>();

        [JsonProperty] public bool AllowedAsRoot { get; private set; }
        [JsonProperty] public List<string> AllowedChildArchetypes { get; private set; } = new List<string>();
        [JsonProperty] public List<MetaAction> AllowedActions { get; set; } = new List<MetaAction>();
        [JsonProperty] public List<MetaState> AllowedStates { get; set; } = new List<MetaState>();
        [JsonProperty] public List<MetaContainer> Containers { get; set; } = new List<MetaContainer>();
        [JsonProperty] public bool IsElement { get; private set; }

        [JsonConstructor] public MetaObj() { }

        public MetaObj(Type type)
        {
            Archetype = type.Name;
            QualifiedClassName = type.AssemblyQualifiedName;
            Archetypes = type.GetArchetypes();
        }

        public MetaObj(string archetype)
        {
            Archetype = archetype;
            Archetypes = Array.Empty<string>();
        }

        public MetaObj SetIsElement(bool isElement)
        {
            IsElement = isElement;
            return this;
        }

        public MetaObj AddIcon(string icon)
        {
            Icon = icon;
            return this;
        }

        public MetaObj AddProp(string prop, EditorType editor, string? dataTypeAlias = null, string? tab = null, string? group = null)
        {
            if (!Props.Any(x => x.FullProp == prop))
                Props.Add(new MetaProp(prop, editor, dataTypeAlias, tab, group));

            return this;
        }

        public MetaObj AddAllowedArchetype(string archetype)
        {
            if (!AllowedChildArchetypes.Any(x => x == archetype))
                AllowedChildArchetypes.Add(archetype);

            return this;
        }

        public MetaObj AllowAsRoot(bool allow)
        {
            AllowedAsRoot = allow;
            return this;
        }

        public MetaObj Merge(MetaObj other)
        {
            foreach (var prop in other.Props)
                if (!Props.Any(x => x.FullProp == prop.FullProp))
                    Props.Add(prop.Clone());

            Archetype ??= other.Archetype;
            Archetypes ??= other.Archetypes;
            Icon ??= other.Icon;
            IsElement = other.IsElement;

            foreach (var action in other.AllowedActions)
            {
                if (!AllowedActions.Any(x => x.Name == action.Name))
                    AllowedActions.Add(action);
            }

            foreach (var state in other.AllowedStates)
            {
                if (!AllowedStates.Any(x => x.Name == state.Name))
                    AllowedStates.Add(state);
            }

            foreach (var child in other.AllowedChildArchetypes)
            {
                if (!AllowedChildArchetypes.Contains(child))
                    AllowedChildArchetypes.Add(child);
            }

            return this;
        }
    }
}
