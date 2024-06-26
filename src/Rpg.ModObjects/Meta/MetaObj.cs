using Newtonsoft.Json;
using Rpg.ModObjects.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Meta
{
    public class MetaObj
    {
        [JsonProperty] public string Archetype { get; private set; }
        [JsonProperty] public string[] Archetypes {  get; private set; }
        [JsonProperty] public string? Icon { get; private set; }
        [JsonProperty] public List<MetaProp> Props { get; set; } = new List<MetaProp>();

        [JsonProperty] public bool AllowedAsRoot { get; private set; }
        [JsonProperty] public List<string> AllowedChildArchetypes { get; private set; } = new List<string>();
        [JsonProperty] public List<MetaAction> AllowedActions { get; set; } = new List<MetaAction>();
        [JsonProperty] public List<MetaState> AllowedStates { get; set; } = new List<MetaState>();
        [JsonProperty] public bool IsElement { get; private set; }

        [JsonConstructor] private MetaObj() { }

        public MetaObj(Type type)
        {
            Archetype = type.Name;
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

        public MetaObj AddProp(string prop, string type)
        {
            if (!Props.Any(x => x.Prop == prop))
                Props.Add(new MetaProp 
                { 
                    Prop = prop, 
                    DataType = type,
                    ReturnType = type,
                    DisplayName = prop 
                });

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
    }
}
