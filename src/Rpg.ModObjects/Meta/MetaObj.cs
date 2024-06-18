using Newtonsoft.Json;
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
        [JsonProperty] public string? Icon { get; private set; }
        [JsonProperty] public List<string> AllowedChildArchetypes { get; private set; } = new List<string>();
        [JsonProperty] public bool AllowedAsRoot {  get; private set; }
        [JsonProperty] public List<MetaProp> Props { get; set; } = new List<MetaProp>();

        [JsonConstructor] private MetaObj() { }

        public MetaObj(string archetype)
        {
            Archetype = archetype;
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
